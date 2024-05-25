﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Kopernicus;
using Kopernicus.ConfigParser.BuiltinTypeParsers;

namespace SigmaDimensionsPlugin
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    internal class SigmaDimensions : MonoBehaviour
    {
        double resize = 1;
        double landscape = 1;
        double resizeBuildings = 1;
        CelestialBody body = null;

        void Start()
        {
            for (int i = 0; i < FlightGlobals.Bodies?.Count; i++)
            {
                body = FlightGlobals.Bodies[i];

                Debug.Log("SigmaDimensions.Start", "> Planet: " + body.name + (body.name != body.displayName.Replace("^N", "") ? (", (A.K.A.: " + body.displayName.Replace("^N", "") + ")") : "") + (body.name != body.transform.name ? (", (A.K.A.: " + body.transform.name + ")") : ""));

                // Sigma Dimensions Settings
                resize = body.Has("resize") ? body.Get<double>("resize") : 1;
                landscape = body.Has("landscape") ? body.Get<double>("landscape") : 1;
                resizeBuildings = body.Has("resizeBuildings") ? body.Get<double>("resizeBuildings") : 1;

                // Excluded mods
                List<object> ExcludeList = body.Has("ExcludedPQSCityMods") ? body.Get<List<object>>("ExcludedPQSCityMods") : new List<object>();

                // All PQSCity mods
                PQSCity[] cities = body.GetComponentsInChildren<PQSCity>(true);

                for (int j = 0; j < cities?.Length; j++)
                {
                    if (!ExcludeList.Contains(cities[j]))
                    {
                        CityFixer(cities[j]);
                        cities[j].Orientate();
                    }
                }

                // All PQSCity2 mods
                PQSCity2[] cities2 = body.GetComponentsInChildren<PQSCity2>(true);

                for (int j = 0; j < cities2?.Length; j++)
                {
                    if (!ExcludeList.Contains(cities2[j]))
                    {
                        City2Fixer(cities2[j]);
                        cities2[j].Orientate();
                    }
                }
            }
        }

        void CityFixer(PQSCity city)
        {
            Debug.Log("SigmaDimensions.CityFixer", "    > PQSCity: " + city.name);

            // Resize the Building
            city.transform.localScale *= (float)resizeBuildings;

            // Fix PQSCity Groups
            if (body.Has("PQSCityGroups"))
            {
                Dictionary<object, Vector3> PQSList = body.Get<Dictionary<object, Vector3>>("PQSCityGroups");
                if (PQSList.ContainsKey(city))
                {
                    GroupFixer(city, PQSList[city]);
                }
            }

            // Add PQSCityFixer Component
            PQSCityFixer fixer = city.gameObject.AddOrGetComponent<PQSCityFixer>();

            // Terrain
            double groundLevel = body.pqsController.GetSurfaceHeight(city.repositionRadial) - body.Radius;
            Debug.Log("SigmaDimensions.CityFixer", "        > Ground Level at Mod (GETSURFACE) = " + groundLevel);
            double oceanOffset = body.ocean && groundLevel < 0 ? groundLevel : 0d;
            Debug.Log("SigmaDimensions.CityFixer", "        > Ocean Offset at Mod = " + oceanOffset);
            groundLevel = body.ocean && groundLevel < 0 ? 0d : groundLevel;
            Debug.Log("SigmaDimensions.CityFixer", "        > Ground Level at Mod (WITH OCEAN) = " + groundLevel);

            // Fix Altitude
            if (!city.repositionToSphere && !city.repositionToSphereSurface)
            {
                // Offset = Distance from the center of the planet
                // THIS IS NOT POSSIBLE AS OF KSP 1.7.1

                Debug.Log("SigmaDimensions.CityFixer", "        > PQSCity Original Center Offset = " + city.repositionRadiusOffset);

                double builtInOffset = city.repositionRadiusOffset - (groundLevel + oceanOffset) / (resize * landscape) - body.Radius / resize;

                Debug.Log("SigmaDimensions.CityFixer", "        > Builtin Offset = " + builtInOffset);

                city.repositionRadiusOffset = body.Radius + groundLevel + oceanOffset + builtInOffset * resizeBuildings;

                Debug.Log("SigmaDimensions.CityFixer", "        > PQSCity Fixed Center Offset = " + city.repositionRadiusOffset);
            }
            else if (city.repositionToSphere && !city.repositionToSphereSurface)
            {
                // Offset = Distance from the radius of the planet

                Debug.Log("SigmaDimensions.CityFixer", "        > PQSCity Original Radius Offset = " + city.repositionRadiusOffset);

                double builtInOffset = city.repositionRadiusOffset - groundLevel / (resize * landscape);

                Debug.Log("SigmaDimensions.CityFixer", "        > Builtin Offset = " + builtInOffset);

                city.repositionRadiusOffset = groundLevel + builtInOffset * resizeBuildings;

                Debug.Log("SigmaDimensions.CityFixer", "        > PQSCity Fixed Radius Offset = " + city.repositionRadiusOffset);
            }
            else
            {
                // Offset = Distance from the surface of the planet

                if (!city.repositionToSphereSurfaceAddHeight)
                {
                    city.repositionToSphereSurfaceAddHeight = true;
                    city.repositionRadiusOffset = 0;
                }

                Debug.Log("SigmaDimensions.CityFixer", "        > PQSCity Original Surface Offset = " + city.repositionRadiusOffset);

                city.repositionRadiusOffset *= resizeBuildings;

                Debug.Log("SigmaDimensions.CityFixer", "        > PQSCity Fixed Surface Offset = " + city.repositionRadiusOffset);
            }
        }

        void City2Fixer(PQSCity2 city)
        {
            Debug.Log("SigmaDimensions.City2Fixer", "    > PQSCity2: " + city.name);

            // Resize the Building
            city.transform.localScale *= (float)resizeBuildings;

            // Fix PQSCity Groups
            if (body.Has("PQSCityGroups"))
            {
                Dictionary<object, Vector3> PQSList = body.Get<Dictionary<object, Vector3>>("PQSCityGroups");
                if (PQSList.ContainsKey(city))
                {
                    GroupFixer(city, PQSList[city]);
                }
            }

            // Add PQSCity2Fixer Component
            PQSCity2Fixer fixer = city.gameObject.AddOrGetComponent<PQSCity2Fixer>();

            // Terrain
            double groundLevel = body.pqsController.GetSurfaceHeight(city.PlanetRelativePosition) - body.Radius;
            Debug.Log("SigmaDimensions.City2Fixer", "        > Ground Level at Mod (GETSURFACE) = " + groundLevel);
            double oceanOffset = body.ocean && groundLevel < 0 ? groundLevel : 0d;
            Debug.Log("SigmaDimensions.City2Fixer", "        > Ocean Offset at Mod = " + oceanOffset);
            groundLevel = body.ocean && groundLevel < 0 ? 0d : groundLevel;
            Debug.Log("SigmaDimensions.City2Fixer", "        > Ground Level at Mod (WITH OCEAN) = " + groundLevel);

            // Fix Altitude
            if (!city.snapToSurface)
            {
                // Offset = Distance from the radius of the planet

                Debug.Log("SigmaDimensions.City2Fixer", "        > PQSCity Original Radius Offset = " + city.alt);

                double builtInOffset = city.alt - groundLevel / (resize * landscape);

                Debug.Log("SigmaDimensions.City2Fixer", "        > Builtin Offset = " + builtInOffset);

                city.alt = groundLevel + builtInOffset * resizeBuildings;

                Debug.Log("SigmaDimensions.City2Fixer", "        > PQSCity Fixed Radius Offset = " + city.alt);
            }
            else
            {
                // Offset = Distance from the surface of the planet

                Debug.Log("SigmaDimensions.City2Fixer", "        > PQSCity Original Surface Offset = " + city.snapHeightOffset);

                city.snapHeightOffset *= resizeBuildings;

                Debug.Log("SigmaDimensions.City2Fixer", "        > PQSCity Fixed Surface Offset = " + city.snapHeightOffset);
            }
        }

        void GroupFixer(object mod, Vector3 REFvector)
        {
            // Moves the group
            Vector3 PQSposition = ((Vector3)GetPosition(mod));
            Debug.Log("SigmaDimensions.City2Fixer", "        > Group center position = " + REFvector + ", (LAT: " + new LatLon(REFvector).lat + ", LON: " + new LatLon(REFvector).lon + ")");
            Debug.Log("SigmaDimensions.City2Fixer", "        > Mod original position = " + PQSposition + ", (LAT: " + new LatLon(PQSposition).lat + ", LON: " + new LatLon(PQSposition).lon + ")");

            if (body == FlightGlobals.GetHomeBody() && REFvector == new Vector3(157000, -1000, -570000))
            {
                PQSCity KSC = body.GetComponentsInChildren<PQSCity>().FirstOrDefault(m => m.name == "KSC");
                MoveGroup(mod, KSC.repositionRadial, KSC.reorientFinalAngle - (-15), 0, 64.7846885412);
                REFvector = KSC.repositionRadial; // Change the REFvector the the new position for Lerping
            }
            else if (body.Has("PQSCityGroupsMove"))
            {
                var MovesInfo = body.Get<Dictionary<Vector3, KeyValuePair<Vector3, NumericParser<double>[]>>>("PQSCityGroupsMove");

                if (MovesInfo.ContainsKey(REFvector))
                {
                    var info = MovesInfo[REFvector];
                    MoveGroup(mod, info.Key, (float)info.Value[0], info.Value[1], info.Value[2]);
                    REFvector = info.Key; // Change the REFvector the the new position for Lerping
                }
            }

            // Update PQSposition
            PQSposition = ((Vector3)GetPosition(mod));

            // Spread or Shrinks the group to account for Resize
            Vector3 NEWvector = Vector3.LerpUnclamped(REFvector.normalized, PQSposition.normalized, (float)(resizeBuildings / resize));
            SetPosition(mod, NEWvector);
            Debug.Log("SigmaDimensions.City2Fixer", "        > Mod lerped position   = " + (Vector3)GetPosition(mod) + ", (LAT: " + new LatLon((Vector3)GetPosition(mod)).lat + ", LON: " + new LatLon((Vector3)GetPosition(mod)).lon + ")");
        }

        void MoveGroup(object mod, Vector3 moveTo, float angle = 0, double fixAltitude = 0, double originalAltitude = double.NegativeInfinity)
        {
            LatLon target = new LatLon(moveTo.normalized);

            // Fix Rotation
            Rotate(mod, angle);

            // ORIGINAL VECTORS (Center, North, East)
            LatLon origin = new LatLon(body.Get<Dictionary<object, Vector3>>("PQSCityGroups")[mod].normalized);
            Vector3 north = Vector3.ProjectOnPlane(Vector3.up, origin.vector);
            Vector3 east = QuaternionD.AngleAxis(90, origin.vector) * north;

            // PQS Vectors (PQS, North, East)
            Vector3 oldPQS = Vector3.ProjectOnPlane(((Vector3)GetPosition(mod)).normalized, origin.vector);
            Vector3 pqsNorth = Vector3.Project(oldPQS, north);
            Vector3 pqsEast = Vector3.Project(oldPQS, east);

            // Distance from center (Northward, Eastward)
            float northward = pqsNorth.magnitude * (1 - (Vector3.Angle(north.normalized, pqsNorth.normalized) / 90));
            float eastward = pqsEast.magnitude * (1 - (Vector3.Angle(east.normalized, pqsEast.normalized) / 90));

            // New Position Vectors (North, East)
            Vector3 newNorth = Vector3.ProjectOnPlane(Vector3.up, target.vector).normalized;
            Vector3 newEast = (QuaternionD.AngleAxis(90, target.vector) * newNorth);

            // Account for PQSCity rotation:
            // PQSCity rotate when their Longitude changes
            angle -= (float)(origin.lon - target.lon);
            QuaternionD rotation = QuaternionD.AngleAxis(angle, target.vector);

            // Calculate final position by adding the north and east distances to the target position
            // then rotate the new vector by as many degrees as it is necessary to account for the PQS model rotation
            SetPosition(mod, rotation * (target.vector + newNorth * northward + newEast * eastward));
            Debug.Log("SigmaDimensions.MoveGroup", "        > Group final position  = " + target.vector + ", (LAT: " + target.lat + ", LON: " + target.lon + ")");
            Debug.Log("SigmaDimensions.MoveGroup", "        > Mod final position    = " + (Vector3)GetPosition(mod) + ", (LAT: " + new LatLon((Vector3)GetPosition(mod)).lat + ", LON: " + new LatLon((Vector3)GetPosition(mod)).lon + ")");

            // Fix Altitude
            if (originalAltitude == double.NegativeInfinity)
            {
                originalAltitude = (body.pqsController.GetSurfaceHeight(origin.vector) - body.Radius) / (resize * landscape);
            }
            Debug.Log("SigmaDimensions.MoveGroup", "        > Mod original altitude = " + originalAltitude);

            FixAltitude(mod, (body.pqsController.GetSurfaceHeight(target.vector) - body.Radius) / (resize * landscape) - originalAltitude, fixAltitude);
        }

        Vector3? GetPosition(object mod)
        {
            string type = mod.GetType().ToString();
            if (type == "PQSCity")
            {
                return ((PQSCity)mod).repositionRadial;
            }
            else if (type == "PQSCity2")
            {
                return ((PQSCity2)mod).PlanetRelativePosition;
            } 
            return null;
        }

        void SetPosition(object mod, Vector3 position)
        {
            string type = mod.GetType().ToString();
            if (type == "PQSCity")
            {
                ((PQSCity)mod).repositionRadial = position;
            }
            else if (type == "PQSCity2")
            {
                LatLon LLA = new LatLon(position);
                ((PQSCity2)mod).lat = LLA.lat;
                ((PQSCity2)mod).lon = LLA.lon;
            }
        }

        void FixAltitude(object mod, double terrainShift, double fixAltitude)
        {
            Debug.Log("SigmaDimensions.FixAltitude", "        > Terrain Shift = " + terrainShift);

            string type = mod.GetType().ToString();
            if (type == "PQSCity")
            {
                PQSCity pqs = (PQSCity)mod;
                if (!pqs.repositionToSphere && !pqs.repositionToSphereSurface)
                {
                    pqs.repositionToSphereSurface = true;
                    pqs.repositionRadiusOffset = 0;
                }
                if (pqs.repositionToSphereSurface && !pqs.repositionToSphereSurfaceAddHeight)
                {
                    pqs.repositionToSphereSurfaceAddHeight = true;
                    pqs.repositionRadiusOffset = 0;
                }
                if (!pqs.repositionToSphereSurface)
                {
                    pqs.repositionRadiusOffset += terrainShift;
                }

                Debug.Log("SigmaDimensions.FixAltitude", "        > Mod original repositionRadiusOffset = " + pqs.repositionRadiusOffset);
                pqs.repositionRadiusOffset += fixAltitude;
                Debug.Log("SigmaDimensions.FixAltitude", "        > Fixed repositionRadiusOffset = " + pqs.repositionRadiusOffset);
            }
            else if (type == "PQSCity2")
            {
                PQSCity2 pqs = (PQSCity2)mod;
                if (pqs.snapToSurface)
                {
                    pqs.snapHeightOffset += fixAltitude;
                    Debug.Log("SigmaDimensions.FixAltitude", "        > Fixed snapHeightOffset = " + pqs.snapHeightOffset);
                }
                else
                {
                    pqs.alt += terrainShift + fixAltitude;
                    Debug.Log("SigmaDimensions.FixAltitude", "        > Fixed PQSCity2.alt = " + pqs.alt);
                }
            }
        }

        void Rotate(object mod, float angle)
        {
            string type = mod.GetType().ToString();
            if (type == "PQSCity")
            {
                ((PQSCity)mod).reorientFinalAngle += angle;
            }
            else if (type == "PQSCity2")
            {
                ((PQSCity2)mod).rotation += angle;
            }
        }

        //from CashnipLeaf:
        //TODO: what even is this? can I remove this?
        public class LatLon
        {
            double[] data = { 1, 1, 1 };
            Vector3 v = Vector3.one;

            public double lat
            {
                get => data[0];
                set
                {
                    data[0] = value;
                    Update();
                }
            }

            public double lon
            {
                get => data[1];
                set
                {
                    data[1] = value;
                    Update();
                }
            }

            public double alt
            {
                get => data[2];
                set
                {
                    data[2] = value;
                    Update();
                }
            }

            public Vector3 vector
            {
                get => v;
                set
                {
                    v = value;
                    data[0] = 90 + Math.Atan2(-v.z / Math.Sin(Math.Atan2(v.z, v.x)), v.y) * 180 / Math.PI;
                    data[1] = Math.Atan2(v.z, v.x) * 180 / Math.PI;
                    data[2] = Math.Pow(v.x * v.x + v.y * v.y + v.z * v.z, 0.5);
                }
            }

            void Update()
            {
                v = Utility.LLAtoECEF(data[0], data[1], 0, data[2]);
            }

            public LatLon()
            {
            }

            public LatLon(Vector3 input)
            {
                vector = input;
            }

            public LatLon(LatLon input)
            {
                data[0] = input.lat;
                data[1] = input.lon;
                data[2] = input.alt;
                v = input.vector;
            }
        }
    }
}
