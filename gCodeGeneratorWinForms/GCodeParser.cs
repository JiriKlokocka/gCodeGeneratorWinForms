using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace gCodeGeneratorWinForms
{
    internal static class GCodeParser
    {
        // Use InvariantCulture everywhere so decimal dot works regardless of locale (e.g. Czech)
        private static readonly CultureInfo CI = CultureInfo.InvariantCulture;


        // ─── G-CODE PARSER ───────────────────────────────────────────────────
        public static List<Segment> ParseGCode(string gcode)
        {
            var segments = new List<Segment>();
            float x = 0, z = 0;
            bool isRapid = false;

            foreach (string rawLine in gcode.Split('\n'))
            {
                string line = rawLine.Trim().ToUpper();
                if (line.StartsWith(";") || string.IsNullOrEmpty(line)) continue;

                // Detect rapid / feed mode — check full command carefully
                string cmd = line.Split(' ')[0]; // get just the G/M code part
                if (cmd == "G0" || cmd == "G00") isRapid = true;
                if (cmd == "G1" || cmd == "G01") isRapid = false;

                float newX = x, newZ = z;
                if (TryGetVal(line, 'X', out float px)) newX = px;
                if (TryGetVal(line, 'Z', out float pz)) newZ = pz;

                bool isArcCW = line.StartsWith("G02");
                bool isArcCCW = line.StartsWith("G03");

                if (isArcCW || isArcCCW)
                {
                    TryGetVal(line, 'I', out float iOff);
                    TryGetVal(line, 'K', out float kOff);

                    float cx = x + iOff;
                    float cz = z + kOff;
                    float r = (float)Math.Sqrt((x - cx) * (x - cx) + (z - cz) * (z - cz));

                    float startAngle = (float)(Math.Atan2(z - cz, x - cx) * 180.0 / Math.PI);
                    float endAngle = (float)(Math.Atan2(newZ - cz, newX - cx) * 180.0 / Math.PI);



                    float sweep = endAngle - startAngle;

                    //Original Claude code
                    //if (isArcCW && sweep > 0) sweep -= 360;
                    //if (isArcCCW && sweep < 0) sweep += 360;

                    //New Code
                    if (sweep < 0)
                    {
                        sweep = startAngle - endAngle;
                        sweep -= 180;
                    }

                    segments.Add(new Segment
                    {
                        Type = MoveType.Arc,
                        Start = new PointF(x, z),
                        End = new PointF(newX, newZ),
                        IsArc = true,
                        Center = new PointF(cx, cz),
                        Radius = r,
                        StartAngle = startAngle,
                        SweepAngle = sweep
                    });
                }
                else if (newX != x || newZ != z)
                {
                    segments.Add(new Segment
                    {
                        Type = isRapid ? MoveType.Rapid : MoveType.Feed,
                        Start = new PointF(x, z),
                        End = new PointF(newX, newZ)
                    });
                }

                x = newX;
                z = newZ;
            }
            return segments;
        }

        private static bool TryGetVal(string line, char axis, out float val)
        {
            val = 0;
            var m = Regex.Match(line, $@"{axis}(-?\d+\.?\d*)");
            if (m.Success)
            {
                val = float.Parse(m.Groups[1].Value, CI);
                return true;
            }
            return false;
        }
    }
}
