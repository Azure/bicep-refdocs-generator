param location string = 'westeurope'
param resource_name string = 'acctest0001'

resource firewallRule 'Microsoft.DBforMariaDB/servers/firewallRules@2018-06-01' = {
  parent: server
  name: resource_name
  properties: {
    endIpAddress: '255.255.255.255'
    startIpAddress: '0.0.0.0'
  }
}

resource server 'Microsoft.DBforMariaDB/servers@2018-06-01' = {
  location: location
  name: resource_name
  properties: {
    administratorLogin: 'acctestun'
    administratorLoginPassword: 'H@Sh1CoR3!'
    createMode: 'Default'
    minimalTlsVersion: 'TLS1_2'
    publicNetworkAccess: 'Enabled'
    sslEnforcement: 'Enabled'
    storageProfile: {
      backupRetentionDays: 7
      storageAutogrow: 'Enabled'
      storageMB: 51200
    }
    version: '10.2'
  }
  sku: {
    capacity: 2
    family: 'Gen5'
    name: 'GP_Gen5_2'
    tier: 'GeneralPurpose'
  }
}

