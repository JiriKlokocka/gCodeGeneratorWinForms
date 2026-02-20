using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace gCodeGeneratorWinForms
{
    internal class TurningParameters
    {
        public double Length { get; set; } = 20.0;
        public double InitialDiameter { get; set; } = 20.0;
        public double TargetDiameter { get; set; } = 10.0;
        private double _cut = 0.5;
        public double Cut
        {
            get => _cut;
            set => _cut = value > 0 ? value : 0.5; // Ensure cut is positive
        }
        public double RoughFeed { get; set; } = 200;
        public double FinishFeed { get; set; } = 150;

        public double LeftSideRadius { get; set; } = 5.0;  // Cannot be greater than depth
        public bool LeftSideIsChamfer { get; set; } = false; // true = chamfer instead of arc

        // RightRadius: positive = outer radius, negative = inner radius
        // if positive  → cannot be greater than targetDiameter / 2
        // if negative  → absolute value cannot be greater than depth
        public double RightSideRadius { get; set; } = 5.0;
        public bool RightSideIsChamfer { get; set; } = false;
        public bool AutoRadiuses { get; set; } = false;
        public double Clearance { get; set; } = 5.0;


        public bool ShowArrows { get; set; } = true;
        [JsonIgnore]
        public bool LastCutTest { get; set; } = false;

        public string FileName { get; set; } = @"C:\Mach3\GCode\_myFile.gcode";
    }
}
