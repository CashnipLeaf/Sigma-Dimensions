﻿using System;
using UnityEngine;
using Kopernicus;


namespace SigmaDimensionsPlugin
{
    public class PQSCityFixer : MonoBehaviour
    {
        void Update()
        {
            CelestialBody body = FlightGlobals.currentMainBody;
            if (body == null) return;

            PQS pqs = body.pqsController;
            if (pqs == null) return;

            PQSCity city = GetComponent<PQSCity>();
            if (city == null) return;

            // Location
            Vector3 planet = body.transform.position;
            Vector3 building = city.transform.position; // From body to city
            Vector3 location = (building - planet).normalized;

            // Sigma Dimensions Settings
            double resize = body.Has("resize") ? body.Get<double>("resize") : 1;
            double landscape = body.Has("landscape") ? body.Get<double>("landscape") : 1;
            double resizeBuildings = body.Has("resizeBuildings") ? body.Get<double>("resizeBuildings") : 1;

            // Max distance
            double maxDistance = Math.Abs(2 * pqs.mapMaxHeight);
            maxDistance *= resize * landscape > 1 ? resize * landscape : 1;
            maxDistance += body.Radius;


            RaycastHit[] hits = Physics.RaycastAll(planet + location * (float)maxDistance, -location, (float)maxDistance, LayerMask.GetMask("Local Scenery"));

            for (int i = 0; i < hits?.Length; i++)
            {
                if (hits[i].collider?.GetComponent<PQ>())
                {
                    Debug.Log("PQSCityFixer", "> Planet: " + body.transform.name);
                    Debug.Log("PQSCityFixer", "    > PQSCity: " + city);

                    // PQSCity parameters
                    double groundLevel = (hits[i].point - planet).magnitude - body.Radius;
                    Debug.Log("PQSCityFixer", "        > Ground Level at Mod (RAYCAST) = " + groundLevel);
                    double error = pqs.GetSurfaceHeight(city.repositionRadial) - body.Radius - groundLevel;
                    Debug.Log("PQSCityFixer", "        > Ground Level Error at Mod = " + error);
                    double oceanDepth = body.ocean && groundLevel < 0 ? -groundLevel : 0d;
                    Debug.Log("PQSCityFixer", "        > Ocean Depth at Mod = " + oceanDepth);
                    groundLevel = body.ocean && groundLevel < 0 ? 0d : groundLevel;
                    Debug.Log("PQSCityFixer", "        > Ground Level at Mod (NEW) = " + groundLevel);

                    // Fix Altitude
                    if (city.repositionToSphere && !city.repositionToSphereSurface)
                    {
                        // Offset = Distance from the radius of the planet

                        Debug.Log("PQSCityFixer", "        > PQSCity Original Radius Offset = " + city.repositionRadiusOffset);

                        double builtInOffset = city.repositionRadiusOffset - groundLevel / (resize * landscape);

                        Debug.Log("PQSCityFixer", "        > Builtuin Offset = " + builtInOffset);

                        city.repositionRadiusOffset = groundLevel + error / (resize * landscape) - (groundLevel + error - city.repositionRadiusOffset) / resizeBuildings;

                        Debug.Log("PQSCityFixer", "        > PQSCity Fixed Radius Offset = " + city.repositionRadiusOffset);
                    }
                    else
                    {
                        // Offset = Distance from the surface of the planet
                        if (!city.repositionToSphereSurface)
                        {
                            city.repositionToSphereSurface = true;
                            city.repositionRadiusOffset = 0;
                        }
                        if (!city.repositionToSphereSurfaceAddHeight)
                        {
                            city.repositionToSphereSurfaceAddHeight = true;
                            city.repositionRadiusOffset = 0;
                        }

                        Debug.Log("PQSCityFixer", "        > PQSCity Original Surface Offset = " + city.repositionRadiusOffset);

                        city.repositionRadiusOffset = oceanDepth + error / (resize * landscape) - (oceanDepth + error - city.repositionRadiusOffset) / resizeBuildings;

                        Debug.Log("PQSCityFixer", "        > PQSCity Fixed Surface Offset = " + city.repositionRadiusOffset);
                    }

                    city.Orientate();
                    DestroyImmediate(this);
                }
            }
        }
    }

    public class PQSCity2Fixer : MonoBehaviour
    {
        void Update()
        {
            CelestialBody body = FlightGlobals.currentMainBody;
            if (body == null) return;

            PQS pqs = body.pqsController;
            if (pqs == null) return;

            PQSCity2 city = GetComponent<PQSCity2>();
            if (city == null) return;

            // Location
            Vector3 planet = body.transform.position;
            Vector3 building = city.transform.position; // From body to city
            Vector3 location = (building - planet).normalized;

            // Sigma Dimensions Settings
            double resize = body.Has("resize") ? body.Get<double>("resize") : 1;
            double landscape = body.Has("landscape") ? body.Get<double>("landscape") : 1;
            double resizeBuildings = body.Has("resizeBuildings") ? body.Get<double>("resizeBuildings") : 1;

            // Max distance
            double maxDistance = Math.Abs(2 * pqs.mapMaxHeight);
            maxDistance *= resize * landscape > 1 ? resize * landscape : 1;
            maxDistance += body.Radius;


            RaycastHit[] hits = Physics.RaycastAll(planet + location * (float)maxDistance, -location, (float)maxDistance, LayerMask.GetMask("Local Scenery"));

            for (int i = 0; i < hits?.Length; i++)
            {
                if (hits[i].collider?.GetComponent<PQ>())
                {
                    Debug.Log("PQSCity2Fixer", "> Planet: " + body.transform.name);
                    Debug.Log("PQSCity2Fixer", "    > PQSCity2: " + city);

                    // PQSCity2 parameters
                    double groundLevel = (hits[i].point - planet).magnitude - body.Radius;
                    Debug.Log("PQSCity2Fixer", "        > Ground Level at Mod (RAYCAST) = " + groundLevel);
                    double error = pqs.GetSurfaceHeight(city.PlanetRelativePosition) - body.Radius - groundLevel;
                    Debug.Log("PQSCity2Fixer", "        > Ground Level Error at Mod = " + error);
                    double oceanDepth = body.ocean && groundLevel < 0 ? -groundLevel : 0d;
                    Debug.Log("PQSCity2Fixer", "        > Ocean Depth at Mod = " + oceanDepth);
                    groundLevel = body.ocean && groundLevel < 0 ? 0d : groundLevel;
                    Debug.Log("PQSCity2Fixer", "        > Ground Level at Mod (NEW) = " + groundLevel);

                    // Because, SQUAD
                    city.PositioningPoint.localPosition /= (float)(body.Radius + city.alt);

                    // Fix Altitude
                    if (!city.snapToSurface)
                    {
                        // Alt = Distance from the radius of the planet

                        Debug.Log("PQSCity2Fixer", "        > PQSCity2 Original Radius Offset = " + city.alt);

                        double builtInOffset = city.alt - groundLevel / (resize * landscape);

                        Debug.Log("PQSCity2Fixer", "        > Builtuin Offset = " + builtInOffset);

                        city.alt = groundLevel + error / (resize * landscape) - (groundLevel + error - city.alt) / resizeBuildings;

                        Debug.Log("PQSCity2Fixer", "        > PQSCity2 Fixed Radius Offset = " + city.alt);
                    }
                    else
                    {
                        // Offset = Distance from the surface of the planet

                        Debug.Log("PQSCity2Fixer", "        > PQSCity2 Original Surface Offset = " + city.snapHeightOffset);

                        double newOffset = oceanDepth + error / (resize * landscape) - (oceanDepth + error - city.snapHeightOffset) / resizeBuildings;

                        city.alt += newOffset - city.snapHeightOffset;
                        city.snapHeightOffset = newOffset;

                        Debug.Log("PQSCity2Fixer", "        > PQSCity2 New Surface Offset = " + city.snapHeightOffset);
                    }

                    // Because, SQUAD
                    city.PositioningPoint.localPosition *= (float)(body.Radius + city.alt);

                    // Apply Changes and Destroy
                    city.Orientate();
                    DestroyImmediate(this);
                }
            }
        }
    }
}
