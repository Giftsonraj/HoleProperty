using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace CircularProperty
{
    public class HoleData
    {
        public double Diameter { get; set; }
        public string HoleType { get; set; }
    }
    public class Hole
    {
        public List<HoleData> DowelHoles { get; set; }
        public List<HoleData> ClearanceHoles { get; set; }

        public Hole()
        {
            DowelHoles = new List<HoleData>();
            ClearanceHoles = new List<HoleData>();
        }
        public static Hole ReadFromXml(string xmlFilePath)
        {
            Hole holeData = new Hole();

            try
            {
                XDocument xmlDoc = XDocument.Load(xmlFilePath);

                foreach (var hole in xmlDoc.Descendants("DowelHole").Elements("hole"))
                {
                    double dia = double.Parse(hole.Attribute("dia").Value);

                    holeData.DowelHoles.Add(new HoleData { Diameter = dia, HoleType = "Dowel Hole"/*, Attribute = attribute*/ });
                }

                foreach (var hole in xmlDoc.Descendants("ClearanceHole").Elements("hole"))
                {
                    double dia = double.Parse(hole.Attribute("dia").Value);

                    holeData.ClearanceHoles.Add(new HoleData { Diameter = dia, HoleType = "Clearance Hole"/*, Attribute = attribute*/ });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading XML file: {ex.Message}");
            }

            return holeData;
        }
    }
}
