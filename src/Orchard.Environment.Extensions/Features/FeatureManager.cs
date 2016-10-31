﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.Environment.Extensions.Features
{
    public class FeatureManager : IFeatureManager
    {
        public const string FeatureManagerCacheKey = "FeatureManager:Features";

        public IFeatureInfoList GetFeatures(
            IExtensionInfo extensionInfo,
            IManifestInfo manifestInfo)
        {
            var features = new Dictionary<string, IFeatureInfo>();

            // Features and Dependencies live within this section
            var featuresSection = manifestInfo.ConfigurationRoot.GetSection("features");
            if (featuresSection.Value != null)
            {
                foreach (var featureSection in featuresSection.GetChildren())
                {
                    var featureId = featureSection.Key;

                    var featureDetails = featureSection.GetChildren().ToDictionary(x => x.Key, v => v.Value);

                    var featureName = featureDetails["name"];

                    // TODO (ngm) look at priority
                    var featurePriority = featureDetails.ContainsKey("priority") ?
                            double.Parse(featureDetails["priority"]) : 0D;

                    var featureDependencyIds = featureDetails["dependencies"] != null ?
                    featureDetails["dependencies"]
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(e => e.Trim())
                        .ToArray() : new string[0];

                    var featureInfo = new FeatureInfo(
                        featureId,
                        featureName,
                        featurePriority,
                        extensionInfo,
                        featureDependencyIds);

                    features.Add(featureId, featureInfo);
                }
            }
            else
            {
                // The Extension has only one feature, itself, and that can have dependencies
                var featureId = extensionInfo.ExtensionFileInfo.Name;
                var featureName = manifestInfo.Name;

                var featureDetails = manifestInfo.ConfigurationRoot;

                // TODO (ngm) look at priority
                var featurePriority = 0D;

                var featureDependencyIds = featureDetails["dependencies"] != null ?
                    featureDetails["dependencies"]
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(e => e.Trim())
                        .ToArray() : new string[0];

                var featureInfo = new FeatureInfo(
                    featureId,
                    featureName,
                    featurePriority,
                    extensionInfo,
                    featureDependencyIds);

                features.Add(featureId, featureInfo);
            }

            return new FeatureInfoList(features);
        }
    }
}
