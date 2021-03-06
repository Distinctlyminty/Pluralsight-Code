﻿using System;
using Microsoft.Extensions.Configuration;

namespace AESEncryption

{

    public class ConfigWrapper
    {
        private readonly IConfiguration _config;

        public ConfigWrapper(IConfiguration config)
        {
            _config = config;
        }

        public string SubscriptionId
        {
            get { return _config["SubscriptionId"]; }
        }

        public string ResourceGroup
        {
            get { return _config["ResourceGroup"]; }
        }

        public string AccountName
        {
            get { return _config["AccountName"]; }
        }

        public string AadTenantId
        {
            get { return _config["AadTenantId"]; }
        }

        public string AadClientId
        {
            get { return _config["AadClientId"]; }
        }

        public string AadSecret
        {
            get { return _config["AadSecret"]; }
        }

        public Uri ArmAadAudience
        {
            get { return new Uri(_config["ArmAadAudience"]); }
        }

        public Uri AadEndpoint
        {
            get { return new Uri(_config["AadEndpoint"]); }
        }

        public Uri ArmEndpoint
        {
            get { return new Uri(_config["ArmEndpoint"]); }
        }

        public string Region
        {
            get { return _config["Region"]; }
        }

        public string StorageContainerName
        {
            get { return _config["StorageContainerName"]; }
        }

        public string StorageAccountName
        {
            get { return _config["StorageAccountName"]; }
        }

        public string StorageAccountKey
        {
            get { return _config["StorageAccountKey"]; }
        }

        public string SymmetricKey
        {
            get { return _config["SymmetricKey"]; }
        }
    }
}
