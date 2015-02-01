using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Text; 
using System.Data;
using System.IO;
using AFPEncryption;

namespace process_exactrac
{
    public class ExacTracFile : CSVFile 
    {
        private string [] footer_array;
        private string[] header_array;
        private string f_name;
        private int f_type = 1;
        private long f_id;
        private string kpath;
        public const int NUM_RECONCILE_FIELDS = 13;
        public const int NUM_INVOICE_FIELDS = 9;
        public ExacTracFile(string path, string key_path)
            : base(path, true, true, 0)
        {
            kpath = key_path;
        }

        public ExacTracFile(string key_path)
            : base("", true, true, 0)
        {
            kpath = key_path;
        }

        public string File
        {
            get { return f_name; }
        }

        public long FileID
        {
            get { return f_id; } 
        }

        public DataTable Records
        {
            get { return tbl; }
        }

        public override DataTable GetData(string filename)
        {
            string temp = Path.GetFileName(filename);
            f_name = filename.Replace(temp, "") + "decrypt\\" + temp;
            PGPWrapper pgp = new PGPWrapper();
            pgp.DecryptFile(filename, f_name, "mypublickey.pgp", "private_kr.pgp", "turkeypotpie");
            tbl = base.GetData("Decrypt\\" + Path.GetFileName(f_name));
            header_array = Header.Split(base.delStr);
            f_id = long.Parse(header_array[0]);
            footer_array = Footer.Split(base.delStr);
            return tbl;
        }

 

        public int FileType
        {
            get { return f_type; }
        }

        public string WriteFile(string path, string filename, DataTable dtbl)
        {
            f_type = 1;
            f_name = filename;
            string encrypt_path = path + "\\encrypt\\" + filename;
            path = path + "\\" + filename;
            tbl = dtbl;
            header_array = new string[2];
            footer_array = new string[3];
            header_array[0] = convert_from_date(DateTime.Now, true);
            header_array[1] = "00000000000000"; 
            footer_array[0] =  tbl.Rows.Count.ToString("000000000#");
            footer_array[1] = "0000000000";
            footer_array[2] = "0000000000";
            string buff = write_header();
            for (int i = 0; i < tbl.Rows.Count; i++)
                buff += write_record(tbl.Rows[i]);
            buff += write_footer();
            FileStream fs = new FileStream(encrypt_path, FileMode.Create);
            fs.Write(Utilities.StringToBuff(buff), 0, buff.Length);
            fs.Close();
            PGPWrapper pgp = new PGPWrapper(); 
            pgp.EncryptFile(encrypt_path, path, kpath); 
            return path;

        }

        public DateTime CreatedDateTime
        {
            get
            {
                return convert_to_date(header_array[0], true);
            }
        }

        public DateTime ProcessedDateTime
        {
            get
            {
                return convert_to_date(header_array[1], true);
            }
        }

        public string ProcessedDateTimeString
        {
            get
            { 
                return header_array[1];
            }
        }

        public string CreatedDateTimeString
        {
            get
            {
                return header_array[0];
            }
        }

        public int RecordCount
        {
            get { return int.Parse(footer_array[0]); }
        }

        public int RecordsProcessed
        {
            get { return int.Parse(footer_array[1]); }
        }

        public int ErrorCount
        {
            get { return int.Parse(footer_array[2]); }
        }

        protected DateTime convert_to_date(string dt, bool condensed)
        {
            DateTime ret_val;
            long temp_dt;
            int msec;
            int sec;
            int min;
            int hr;
            int day;
            int mon;
            int yr;
            if (condensed)
            {
                try
                {
                    temp_dt = long.Parse(dt);
                }
                catch
                {
                    throw new Exception("Cannot convert to date. Value '" 
                        + dt + "' is not in correct format.");
                }
                if (temp_dt == 0) return DateTime.Now;
                sec = (int)(temp_dt % 100);
                min = (int)(temp_dt % 10000 / 100);
                hr = (int)(temp_dt % 1000000 / 10000);
                yr = (int)(temp_dt % 10000000000 / 1000000);
                day = (int)(temp_dt % 1000000000000 / 10000000000);
                mon = (int)(temp_dt / 1000000000000);
                ret_val = new DateTime(yr, mon, day, hr, min, sec);

            }
            else
            {
                string[] dt_arr = new string[7];
                dt_arr[0] = dt.Substring(0, 4);
                dt_arr[1] = dt.Substring(5, 2);
                dt_arr[2] = dt.Substring(8, 2);
                dt_arr[3] = dt.Substring(11, 2);
                dt_arr[4] = dt.Substring(14, 2);
                dt_arr[5] = dt.Substring(17, 2);
                dt_arr[6] = dt.Substring(20, 4);
                msec = int.Parse(dt_arr[6]);
                sec = int.Parse(dt_arr[5]);
                min = int.Parse(dt_arr[4]);
                hr = int.Parse(dt_arr[3]);
                day = int.Parse(dt_arr[2]);
                mon =  int.Parse(dt_arr[1]);
                yr  = int.Parse(dt_arr[0]);
                ret_val = new DateTime(yr, mon, day, hr, min, sec, msec);

            }
            return ret_val;
        }

        protected string convert_from_date(DateTime dt, bool condensed)
        {
            if (condensed)
                return dt.Month.ToString("0#") + dt.Day.ToString("0#") +
                    dt.Year.ToString() + dt.Hour.ToString("0#") + dt.Minute.ToString("0#")
                    + dt.Second.ToString("0#");
            else
                return dt.Year.ToString() + "-" + dt.Month.ToString("0#") + "-"
            + dt.Day.ToString("0#") + " " + dt.Hour.ToString("0#") + ":" + dt.Minute.ToString("0#") + ":"
            + dt.Second.ToString("0#") + ":" + dt.Millisecond.ToString("000#");

        }

        protected string write_header()
        {
            return header_array[0] + "," + header_array[1] + "\r\n";
        }

        protected string write_footer()
        {
            return footer_array[0] + "," + footer_array[1] + "," + footer_array[2] + "\r\n";
        }

        protected string write_record(DataRow dr)
        {
            string retval = "";
            for (int i = 0; i < dr.Table.Columns.Count; i++)
            {
                retval += dr[i].ToString();
                if (i < dr.Table.Columns.Count - 1)
                    retval += ",";
            }
            return retval + "\r\n";
        }

        public long LogFile(SQLWrapper sql)
        {
            f_id = sql.LongScalar("exec Automation..spLogExacTracFile "
                + "'" +  File + "','" +  CreatedDateTime.ToString() 
                + "','" +  ProcessedDateTime.ToString() + "'," +  RecordCount.ToString()
            + "," +  RecordsProcessed.ToString() + "," +  ErrorCount.ToString()
            + "," +  FileType.ToString());
            return f_id;
        }

        protected override DataTable GetTableStructure(string[] lines)
        {
            string colname;
            int i;
            offset = 1;
            strHeader = lines[0];
            string first_line = lines[offset];
            string[] cols = first_line.Split(delStr);
            //Determine file type
            if (cols.Length  == NUM_RECONCILE_FIELDS)
                f_type = 2;
            if (cols.Length == NUM_INVOICE_FIELDS)
                f_type = 3;
            DataTable tmp = new DataTable(); 
            for (i = 0; i < cols.Length; i++)
            {
                colname = "column" + i.ToString("00");
                tmp.Columns.Add(colname);

            }
            return tmp;
        }
    }
}
