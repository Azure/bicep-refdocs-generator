param location string = 'westeurope'
param resource_name string = 'acctest0001'

resource authorizationServer 'Microsoft.ApiManagement/service/authorizationServers@2021-08-01' = {
  parent: service
  name: resource_name
  properties: {
    authorizationEndpoint: 'https://azacceptance.hashicorptest.com/client/authorize'
    authorizationMethods: [
      'GET'
    ]
    clientAuthenticationMethod: []
    clientId: '42424242-4242-4242-4242-424242424242'
    clientRegistrationEndpoint: 'https://azacceptance.hashicorptest.com/client/register'
    clientSecret: ''
    defaultScope: ''
    description: ''
    displayName: 'Test Group'
    grantTypes: [
      'implicit'
    ]
    resourceOwnerPassword: ''
    resourceOwnerUsername: ''
    supportState: false
    tokenBodyParameters: []
  }
}

resource service 'Microsoft.ApiManagement/service@2021-08-01' = {
  location: location
  name: resource_name
  properties: {
    certificates: []
    customProperties: {
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Backend.Protocols.Ssl30': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Backend.Protocols.Tls10': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Backend.Protocols.Tls11': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Protocols.Tls10': 'false'
      'Microsoft.WindowsAzure.ApiManagement.Gateway.Security.Protocols.Tls11': 'false'
    }
    disableGateway: false
    publicNetworkAccess: 'Enabled'
    publisherEmail: 'pub1@email.com'
    publisherName: 'pub1'
    virtualNetworkType: 'None'
  }
  sku: {
    capacity: 0
    name: 'Consumption'
  }
}

