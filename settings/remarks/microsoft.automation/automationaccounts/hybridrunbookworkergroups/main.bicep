param location string = 'westeurope'
param resource_name string = 'acctest0001'

resource automationAccount 'Microsoft.Automation/automationAccounts@2021-06-22' = {
  location: location
  name: resource_name
  properties: {
    encryption: {
      keySource: 'Microsoft.Automation'
    }
    publicNetworkAccess: true
    sku: {
      name: 'Basic'
    }
  }
}

resource credential 'Microsoft.Automation/automationAccounts/credentials@2020-01-13-preview' = {
  parent: automationAccount
  name: resource_name
  properties: {
    description: ''
    password: 'test_pwd'
    userName: 'test_user'
  }
}

resource hybridRunbookWorkerGroup 'Microsoft.Automation/automationAccounts/hybridRunbookWorkerGroups@2021-06-22' = {
  parent: automationAccount
  name: resource_name
  credential: {
    name: credential.name
  }
}

