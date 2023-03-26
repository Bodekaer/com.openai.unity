﻿// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Linq;
using UnityEngine;

namespace OpenAI
{
    /// <summary>
    /// The client settings for configuring Azure OpenAI or custom domain.
    /// </summary>
    public sealed class OpenAIClientSettings
    {
        internal const string OpenAIDomain = "api.openai.com";
        internal const string DefaultOpenAIApiVersion = "v1";
        internal const string AzureOpenAIDomain = "openai.azure.com";
        internal const string DefaultAzureApiVersion = "2022-12-01";

        /// <summary>
        /// Creates a new instance of <see cref="OpenAIClientSettings"/> for use with OpenAI.
        /// </summary>
        public OpenAIClientSettings()
        {
            ResourceName = OpenAIDomain;
            ApiVersion = DefaultOpenAIApiVersion;
            DeploymentId = string.Empty;
            BaseRequest = $"/{ApiVersion}/";
            BaseRequestUrlFormat = $"https://{ResourceName}{BaseRequest}{{0}}";
        }

        /// <summary>
        /// Creates a new instance of <see cref="OpenAIClientSettings"/> for use with OpenAI.
        /// </summary>
        /// <param name="domain">Base api domain.</param>
        /// <param name="apiVersion">The version of the OpenAI api you want to use.</param>
        public OpenAIClientSettings(string domain, string apiVersion = DefaultOpenAIApiVersion)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                domain = OpenAIDomain;
            }

            if (!domain.Contains('.') &&
                !domain.Contains(':'))
            {
                throw new ArgumentException($"You're attempting to pass a \"resourceName\" parameter to \"{nameof(domain)}\". Please specify \"resourceName:\" for this parameter in constructor.");
            }

            if (string.IsNullOrWhiteSpace(apiVersion))
            {
                apiVersion = DefaultOpenAIApiVersion;
            }

            ResourceName = domain;
            ApiVersion = apiVersion;
            DeploymentId = string.Empty;
            BaseRequest = $"/{ApiVersion}/";
            BaseRequestUrlFormat = $"https://{ResourceName}{BaseRequest}{{0}}";
        }

        /// <summary>
        /// Creates a new instance of the <see cref="OpenAIClientSettings"/> for use with Azure OpenAI.<br/>
        /// <see href="https://learn.microsoft.com/en-us/azure/cognitive-services/openai/"/>
        /// </summary>
        /// <param name="resourceName">
        /// The name of your Azure OpenAI Resource.
        /// </param>
        /// <param name="deploymentId">
        /// The name of your model deployment. You're required to first deploy a model before you can make calls.
        /// </param>
        /// <param name="apiVersion">
        /// Optional, defaults to 2022-12-01
        /// </param>
        public OpenAIClientSettings(string resourceName, string deploymentId, string apiVersion = DefaultAzureApiVersion)
        {
            if (string.IsNullOrWhiteSpace(resourceName))
            {
                throw new ArgumentNullException(nameof(resourceName));
            }

            if (resourceName.Contains('.') ||
                resourceName.Contains(':'))
            {
                throw new ArgumentException($"You're attempting to pass a \"domain\" parameter to \"{nameof(resourceName)}\". Please specify \"domain:\" for this parameter in constructor.");
            }

            if (string.IsNullOrWhiteSpace(apiVersion))
            {
                apiVersion = DefaultAzureApiVersion;
            }

            ResourceName = resourceName;
            DeploymentId = deploymentId;
            ApiVersion = apiVersion;
            BaseRequest = $"/openai/deployments/{DeploymentId}/";
            BaseRequestUrlFormat = $"https://{ResourceName}.{AzureOpenAIDomain}{BaseRequest}{{0}}?api-version={ApiVersion}";
        }

        public string ResourceName { get; }

        public string ApiVersion { get; }

        public string DeploymentId { get; }

        internal string BaseRequest { get; }

        internal string BaseRequestUrlFormat { get; }

        private static OpenAIClientSettings cachedDefault;

        public static OpenAIClientSettings Default
        {
            get
            {
                if (cachedDefault != null)
                {
                    return cachedDefault;
                }

                var config = Resources.LoadAll<OpenAIConfigurationSettings>(string.Empty).FirstOrDefault(asset => asset != null);

                if (config != null)
                {
                    if (config.UseAzureOpenAI)
                    {
                        cachedDefault = new OpenAIClientSettings(
                            resourceName: config.ResourceName,
                            deploymentId: config.DeploymentId,
                            apiVersion: config.ApiVersion);
                    }
                    else
                    {
                        cachedDefault = new OpenAIClientSettings(
                            domain: config.ProxyDomain,
                            apiVersion: config.ApiVersion);
                    }
                }
                else
                {
                    cachedDefault = new OpenAIClientSettings();
                }

                return cachedDefault;
            }
        }
    }
}
