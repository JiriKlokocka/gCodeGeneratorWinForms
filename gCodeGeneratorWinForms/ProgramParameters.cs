namespace gCodeGeneratorWinForms
{
    internal class ProgramParameters
    {
        public bool ShowArrows { get; set; } = true;
        public bool SymmetricDisplay { get; set; } = false;
        public bool ShowMaterial { get; set; } = true;
        //public bool LastCutTest { get; set; } = false;
        public string FileName { get; set; } = @"C:\Mach3\GCode\_myFile.gcode";
        public string IoSenderPath { get; set; } = "";
        public bool OpenFileInIoSender { get; set; } = false;
    }
}
