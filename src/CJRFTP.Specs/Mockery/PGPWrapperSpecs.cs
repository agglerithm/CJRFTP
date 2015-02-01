namespace CJRFTP.Specs.Mockery
{
    using System;
    using System.Collections.Generic;
    using CJR.Common.IO;
    using Machine.Specifications;
    using Moq;
    using CJR_PGP;
    using It = Machine.Specifications.It;

    public class when_file_is_decrypted
    {
        private static IPGPWrapper _pgp;
        private static Mock<IFileUtilities> _fUtil;
        private static Mock<IPGPKeys> _keys;
        private static Mock<IPGPIO> _files;
        public Establish context = () =>
        {
            _fUtil = new Mock<IFileUtilities>();
            _fUtil.Setup(
                u => u.GetListFromFolder(Moq.It.IsAny<string>(), 
                    Moq.It.IsAny<string>(), 
                    Moq.It.IsAny<DateTime>())).Returns(new List<FileEntity> 
                    { new FileEntity(), new FileEntity()});
            _files = new Mock<IPGPIO>();
            _keys = new Mock<IPGPKeys>();
            _pgp = new PGPWrapper(_fUtil.Object, _keys.Object, _files.Object,
                                  "CJRpublickey.asc", "CJR_private.pgp", "turkeypotpie");
            _pgp.DecryptAll(@"..\..\..\..\TestFiles\Download\Encrypted\", @"..\..\..\..\TestFiles\Download\");
        };

        private It should_result_in_readable_file = () =>
        {

        };

    }
}
