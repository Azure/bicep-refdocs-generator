---
title: Microsoft.CostManagement/exports 2019-01-01
description: Azure Microsoft.CostManagement/exports syntax and properties to use in Azure Resource Manager templates for deploying the resource. API version 2019-01-01
author: tfitzmac
zone_pivot_groups: deployment-languages-reference
ms.service: azure-resource-manager
ms.topic: reference
ms.date: 09/13/2024
ms.author: tomfitz
---
# # Microsoft.CostManagement exports 2019-01-01

> [!div class="op_single_selector" title1="API Versions:"]
> - [Latest](../exports.md)
> - [2023-11-01](../2023-11-01/exports.md)
> - [2023-09-01](../2023-09-01/exports.md)
> - [2023-08-01](../2023-08-01/exports.md)
> - [2023-07-01-preview](../2023-07-01-preview/exports.md)
> - [2023-04-01-preview](../2023-04-01-preview/exports.md)
> - [2023-03-01](../2023-03-01/exports.md)
> - [2022-10-01](../2022-10-01/exports.md)
> - [2021-10-01](../2021-10-01/exports.md)
> - [2021-01-01](../2021-01-01/exports.md)
> - [2020-12-01-preview](../2020-12-01-preview/exports.md)
> - [2020-06-01](../2020-06-01/exports.md)
> - [2019-11-01](../2019-11-01/exports.md)
> - [2019-10-01](../2019-10-01/exports.md)
> - [2019-09-01](../2019-09-01/exports.md)
> - [2019-01-01](../2019-01-01/exports.md)


::: zone pivot="deployment-language-bicep"

## Bicep resource definition

The exports resource type can be deployed with operations that target: 



For a list of changed properties in each API version, see [change log](~/microsoft.costmanagement/change-log/exports.md).

## Resource format

To create a Microsoft.CostManagement/exports resource, add the following Bicep to your template.

```bicep
resource symbolicname 'Microsoft.CostManagement/exports@2019-01-01' = {
  name: 'string'
  properties: {
    definition: {
      dataset: {
        aggregation: {
          {customized property}: {
            function: 'string'
            name: 'string'
          }
        }
        configuration: {
          columns: [
            'string'
          ]
        }
        filter: {
          and: [
            ...
          ]
          dimension: {
            name: 'string'
            operator: 'string'
            values: [
              'string'
            ]
          }
          not: ...
          or: [
            ...
          ]
          tag: {
            name: 'string'
            operator: 'string'
            values: [
              'string'
            ]
          }
        }
        granularity: 'string'
        grouping: [
          {
            name: 'string'
            type: 'string'
          }
        ]
        sorting: [
          {
            name: 'string'
            querySortingDirection: 'string'
          }
        ]
      }
      timeframe: 'string'
      timePeriod: {
        from: 'string'
        to: 'string'
      }
      type: 'string'
    }
    deliveryInfo: {
      destination: {
        container: 'string'
        resourceId: 'string'
        rootFolderPath: 'string'
      }
    }
    format: 'string'
    schedule: {
      recurrence: 'string'
      recurrencePeriod: {
        from: 'string'
        to: 'string'
      }
      status: 'string'
    }
  }
}
```
## Property values

### ExportDeliveryDestination

| Name | Description | Value |
| ---- | ----------- | ------------ |
| container | The name of the container where exports will be uploaded. | string (required) |
| resourceId | The resource id of the storage account where exports will be delivered. | string (required) |
| rootFolderPath | The name of the directory where exports will be uploaded. | string |

### ExportDeliveryInfo

| Name | Description | Value |
| ---- | ----------- | ------------ |
| destination | Has destination for the export being delivered. | [ExportDeliveryDestination](#exportdeliverydestination) (required) |

### ExportProperties

| Name | Description | Value |
| ---- | ----------- | ------------ |
| definition | Has definition for the export. | [QueryDefinition](#querydefinition) (required) |
| deliveryInfo | Has delivery information for the export. | [ExportDeliveryInfo](#exportdeliveryinfo) (required) |
| format | The format of the export being delivered. | 'Csv' |
| schedule | Has schedule information for the export. | [ExportSchedule](#exportschedule) |

### ExportRecurrencePeriod

| Name | Description | Value |
| ---- | ----------- | ------------ |
| from | The start date of recurrence. | string (required) |
| to | The end date of recurrence. | string |

### ExportSchedule

| Name | Description | Value |
| ---- | ----------- | ------------ |
| recurrence | The schedule recurrence. | 'Annually'<br />'Daily'<br />'Monthly'<br />'Weekly' (required) |
| recurrencePeriod | Has start and end date of the recurrence. The start date must be in future. If present, the end date must be greater than start date. | [ExportRecurrencePeriod](#exportrecurrenceperiod) |
| status | The status of the schedule. Whether active or not. If inactive, the export's scheduled execution is paused. | 'Active'<br />'Inactive' |

### Microsoft.CostManagement/exports

| Name | Description | Value |
| ---- | ----------- | ------------ |
| name | The resource name | string (required) |
| properties | The properties of the export. | [ExportProperties](#exportproperties) |

### QueryAggregation

| Name | Description | Value |
| ---- | ----------- | ------------ |
| function | The name of the aggregation function to use. | 'Sum' (required) |
| name | The name of the column to aggregate. | string (required) |

### QueryComparisonExpression

| Name | Description | Value |
| ---- | ----------- | ------------ |
| name | The name of the column to use in comparison. | string (required) |
| operator | The operator to use for comparison. | 'In' (required) |
| values | Array of values to use for comparison | string[] (required) |

### QueryDataset

| Name | Description | Value |
| ---- | ----------- | ------------ |
| aggregation | Dictionary of aggregation expression to use in the query. The key of each item in the dictionary is the alias for the aggregated column. Query can have up to 2 aggregation clauses. | [QueryDatasetAggregation](#querydatasetaggregation) |
| configuration | Has configuration information for the data in the export. The configuration will be ignored if aggregation and grouping are provided. | [QueryDatasetConfiguration](#querydatasetconfiguration) |
| filter | The filter expression to use in the query. Please reference our Query API REST documentation for how to properly format the filter. | [QueryFilter](#queryfilter) |
| granularity | The granularity of rows in the query. | 'Daily'<br />'Hourly' |
| grouping | Array of group by expression to use in the query. Query can have up to 2 group by clauses. | [QueryGrouping](#querygrouping)[] |
| sorting | Array of sorting by columns in query. | [QuerySortingConfiguration](#querysortingconfiguration)[] |

### QueryDatasetAggregation

| Name | Description | Value |
| ---- | ----------- | ------------ |

### QueryDatasetConfiguration

| Name | Description | Value |
| ---- | ----------- | ------------ |
| columns | Array of column names to be included in the query. Any valid query column name is allowed. If not provided, then query includes all columns. | string[] |

### QueryDefinition

| Name | Description | Value |
| ---- | ----------- | ------------ |
| dataset | Has definition for data in this query. | [QueryDataset](#querydataset) |
| timeframe | The time frame for pulling data for the query. If custom, then a specific time period must be provided. | 'BillingMonthToDate'<br />'Custom'<br />'MonthToDate'<br />'TheLastBillingMonth'<br />'TheLastMonth'<br />'TheLastWeek'<br />'TheLastYear'<br />'WeekToDate'<br />'YearToDate' (required) |
| timePeriod | Has time period for pulling data for the query. | [QueryTimePeriod](#querytimeperiod) |
| type | The type of the query. | 'Usage' (required) |

### QueryFilter

| Name | Description | Value |
| ---- | ----------- | ------------ |
| and | The logical "AND" expression. Must have at least 2 items. | [QueryFilter](#queryfilter)[] |
| dimension | Has comparison expression for a dimension | [QueryComparisonExpression](#querycomparisonexpression) |
| not | The logical "NOT" expression. | [QueryFilter](#queryfilter) |
| or | The logical "OR" expression. Must have at least 2 items. | [QueryFilter](#queryfilter)[] |
| tag | Has comparison expression for a tag | [QueryComparisonExpression](#querycomparisonexpression) |

### QueryGrouping

| Name | Description | Value |
| ---- | ----------- | ------------ |
| name | The name of the column to group. | string (required) |
| type | Has type of the column to group. | 'Dimension'<br />'Tag' (required) |

### QuerySortingConfiguration

| Name | Description | Value |
| ---- | ----------- | ------------ |
| name | The name of the column to use in sorting. | string |
| querySortingDirection | The sorting direction | 'Ascending'<br />'Descending' |

### QueryTimePeriod

| Name | Description | Value |
| ---- | ----------- | ------------ |
| from | The start date to pull data from. | string (required) |
| to | The end date to pull data to. | string (required) |



::: zone-end

::: zone pivot="deployment-language-arm-template"

## ARM template resource definition

The exports resource type can be deployed with operations that target: 



For a list of changed properties in each API version, see [change log](~/microsoft.costmanagement/change-log/exports.md).

## Resource format

To create a Microsoft.CostManagement/exports resource, add the following JSON to your template.

```json
{
  "type": "Microsoft.CostManagement/exports",
  "apiVersion": "2019-01-01",
  "name": "string",
  "properties": {
    "definition": {
      "dataset": {
        "aggregation": {
          "{customized property}": {
            "function": "string",
            "name": "string"
          }
        },
        "configuration": {
          "columns": [ "string" ]
        },
        "filter": {
          "and": [
            ...
          ],
          "dimension": {
            "name": "string",
            "operator": "string",
            "values": [ "string" ]
          },
          "not": ...,
          "or": [
            ...
          ],
          "tag": {
            "name": "string",
            "operator": "string",
            "values": [ "string" ]
          }
        },
        "granularity": "string",
        "grouping": [
          {
            "name": "string",
            "type": "string"
          }
        ],
        "sorting": [
          {
            "name": "string",
            "querySortingDirection": "string"
          }
        ]
      },
      "timeframe": "string",
      "timePeriod": {
        "from": "string",
        "to": "string"
      },
      "type": "string"
    },
    "deliveryInfo": {
      "destination": {
        "container": "string",
        "resourceId": "string",
        "rootFolderPath": "string"
      }
    },
    "format": "string",
    "schedule": {
      "recurrence": "string",
      "recurrencePeriod": {
        "from": "string",
        "to": "string"
      },
      "status": "string"
    }
  }
}
```
## Property values

### ExportDeliveryDestination

| Name | Description | Value |
| ---- | ----------- | ------------ |
| container | The name of the container where exports will be uploaded. | string (required) |
| resourceId | The resource id of the storage account where exports will be delivered. | string (required) |
| rootFolderPath | The name of the directory where exports will be uploaded. | string |

### ExportDeliveryInfo

| Name | Description | Value |
| ---- | ----------- | ------------ |
| destination | Has destination for the export being delivered. | [ExportDeliveryDestination](#exportdeliverydestination-1) (required) |

### ExportProperties

| Name | Description | Value |
| ---- | ----------- | ------------ |
| definition | Has definition for the export. | [QueryDefinition](#querydefinition-1) (required) |
| deliveryInfo | Has delivery information for the export. | [ExportDeliveryInfo](#exportdeliveryinfo-1) (required) |
| format | The format of the export being delivered. | 'Csv' |
| schedule | Has schedule information for the export. | [ExportSchedule](#exportschedule-1) |

### ExportRecurrencePeriod

| Name | Description | Value |
| ---- | ----------- | ------------ |
| from | The start date of recurrence. | string (required) |
| to | The end date of recurrence. | string |

### ExportSchedule

| Name | Description | Value |
| ---- | ----------- | ------------ |
| recurrence | The schedule recurrence. | 'Annually'<br />'Daily'<br />'Monthly'<br />'Weekly' (required) |
| recurrencePeriod | Has start and end date of the recurrence. The start date must be in future. If present, the end date must be greater than start date. | [ExportRecurrencePeriod](#exportrecurrenceperiod-1) |
| status | The status of the schedule. Whether active or not. If inactive, the export's scheduled execution is paused. | 'Active'<br />'Inactive' |

### Microsoft.CostManagement/exports

| Name | Description | Value |
| ---- | ----------- | ------------ |
| type | The resource type | 'Microsoft.CostManagement/exports' |
| name | The resource name | string (required) |
| properties | The properties of the export. | [ExportProperties](#exportproperties-1) |

### QueryAggregation

| Name | Description | Value |
| ---- | ----------- | ------------ |
| function | The name of the aggregation function to use. | 'Sum' (required) |
| name | The name of the column to aggregate. | string (required) |

### QueryComparisonExpression

| Name | Description | Value |
| ---- | ----------- | ------------ |
| name | The name of the column to use in comparison. | string (required) |
| operator | The operator to use for comparison. | 'In' (required) |
| values | Array of values to use for comparison | string[] (required) |

### QueryDataset

| Name | Description | Value |
| ---- | ----------- | ------------ |
| aggregation | Dictionary of aggregation expression to use in the query. The key of each item in the dictionary is the alias for the aggregated column. Query can have up to 2 aggregation clauses. | [QueryDatasetAggregation](#querydatasetaggregation-1) |
| configuration | Has configuration information for the data in the export. The configuration will be ignored if aggregation and grouping are provided. | [QueryDatasetConfiguration](#querydatasetconfiguration-1) |
| filter | The filter expression to use in the query. Please reference our Query API REST documentation for how to properly format the filter. | [QueryFilter](#queryfilter-1) |
| granularity | The granularity of rows in the query. | 'Daily'<br />'Hourly' |
| grouping | Array of group by expression to use in the query. Query can have up to 2 group by clauses. | [QueryGrouping](#querygrouping-1)[] |
| sorting | Array of sorting by columns in query. | [QuerySortingConfiguration](#querysortingconfiguration-1)[] |

### QueryDatasetAggregation

| Name | Description | Value |
| ---- | ----------- | ------------ |

### QueryDatasetConfiguration

| Name | Description | Value |
| ---- | ----------- | ------------ |
| columns | Array of column names to be included in the query. Any valid query column name is allowed. If not provided, then query includes all columns. | string[] |

### QueryDefinition

| Name | Description | Value |
| ---- | ----------- | ------------ |
| dataset | Has definition for data in this query. | [QueryDataset](#querydataset-1) |
| timeframe | The time frame for pulling data for the query. If custom, then a specific time period must be provided. | 'BillingMonthToDate'<br />'Custom'<br />'MonthToDate'<br />'TheLastBillingMonth'<br />'TheLastMonth'<br />'TheLastWeek'<br />'TheLastYear'<br />'WeekToDate'<br />'YearToDate' (required) |
| timePeriod | Has time period for pulling data for the query. | [QueryTimePeriod](#querytimeperiod-1) |
| type | The type of the query. | 'Usage' (required) |

### QueryFilter

| Name | Description | Value |
| ---- | ----------- | ------------ |
| and | The logical "AND" expression. Must have at least 2 items. | [QueryFilter](#queryfilter-1)[] |
| dimension | Has comparison expression for a dimension | [QueryComparisonExpression](#querycomparisonexpression-1) |
| not | The logical "NOT" expression. | [QueryFilter](#queryfilter-1) |
| or | The logical "OR" expression. Must have at least 2 items. | [QueryFilter](#queryfilter-1)[] |
| tag | Has comparison expression for a tag | [QueryComparisonExpression](#querycomparisonexpression-1) |

### QueryGrouping

| Name | Description | Value |
| ---- | ----------- | ------------ |
| name | The name of the column to group. | string (required) |
| type | Has type of the column to group. | 'Dimension'<br />'Tag' (required) |

### QuerySortingConfiguration

| Name | Description | Value |
| ---- | ----------- | ------------ |
| name | The name of the column to use in sorting. | string |
| querySortingDirection | The sorting direction | 'Ascending'<br />'Descending' |

### QueryTimePeriod

| Name | Description | Value |
| ---- | ----------- | ------------ |
| from | The start date to pull data from. | string (required) |
| to | The end date to pull data to. | string (required) |



::: zone-end

::: zone pivot="deployment-language-terraform"

## Terraform (AzAPI provider) resource definition

The exports resource type can be deployed with operations that target: 



For a list of changed properties in each API version, see [change log](~/microsoft.costmanagement/change-log/exports.md).

## Resource format

To create a Microsoft.CostManagement/exports resource, add the following Terraform to your template.

```terraform
resource "azapi_resource" "symbolicname" {
  type = "Microsoft.CostManagement/exports@2019-01-01"
  name = "string"
  body = jsonencode({
    properties = {
      definition = {
        dataset = {
          aggregation = {
            {customized property} = {
              function = "string"
              name = "string"
            }
          }
          configuration = {
            columns = [
              "string"
            ]
          }
          filter = {
            and = [
              ...
            ]
            dimension = {
              name = "string"
              operator = "string"
              values = [
                "string"
              ]
            }
            not = ...
            or = [
              ...
            ]
            tag = {
              name = "string"
              operator = "string"
              values = [
                "string"
              ]
            }
          }
          granularity = "string"
          grouping = [
            {
              name = "string"
              type = "string"
            }
          ]
          sorting = [
            {
              name = "string"
              querySortingDirection = "string"
            }
          ]
        }
        timeframe = "string"
        timePeriod = {
          from = "string"
          to = "string"
        }
        type = "string"
      }
      deliveryInfo = {
        destination = {
          container = "string"
          resourceId = "string"
          rootFolderPath = "string"
        }
      }
      format = "string"
      schedule = {
        recurrence = "string"
        recurrencePeriod = {
          from = "string"
          to = "string"
        }
        status = "string"
      }
    }
  })
}
```
## Property values

### ExportDeliveryDestination

| Name | Description | Value |
| ---- | ----------- | ------------ |
| container | The name of the container where exports will be uploaded. | string (required) |
| resourceId | The resource id of the storage account where exports will be delivered. | string (required) |
| rootFolderPath | The name of the directory where exports will be uploaded. | string |

### ExportDeliveryInfo

| Name | Description | Value |
| ---- | ----------- | ------------ |
| destination | Has destination for the export being delivered. | [ExportDeliveryDestination](#exportdeliverydestination-2) (required) |

### ExportProperties

| Name | Description | Value |
| ---- | ----------- | ------------ |
| definition | Has definition for the export. | [QueryDefinition](#querydefinition-2) (required) |
| deliveryInfo | Has delivery information for the export. | [ExportDeliveryInfo](#exportdeliveryinfo-2) (required) |
| format | The format of the export being delivered. | 'Csv' |
| schedule | Has schedule information for the export. | [ExportSchedule](#exportschedule-2) |

### ExportRecurrencePeriod

| Name | Description | Value |
| ---- | ----------- | ------------ |
| from | The start date of recurrence. | string (required) |
| to | The end date of recurrence. | string |

### ExportSchedule

| Name | Description | Value |
| ---- | ----------- | ------------ |
| recurrence | The schedule recurrence. | 'Annually'<br />'Daily'<br />'Monthly'<br />'Weekly' (required) |
| recurrencePeriod | Has start and end date of the recurrence. The start date must be in future. If present, the end date must be greater than start date. | [ExportRecurrencePeriod](#exportrecurrenceperiod-2) |
| status | The status of the schedule. Whether active or not. If inactive, the export's scheduled execution is paused. | 'Active'<br />'Inactive' |

### Microsoft.CostManagement/exports

| Name | Description | Value |
| ---- | ----------- | ------------ |
| type | The resource type | "Microsoft.CostManagement/exports@2019-01-01" |
| name | The resource name | string (required) |
| properties | The properties of the export. | [ExportProperties](#exportproperties-2) |

### QueryAggregation

| Name | Description | Value |
| ---- | ----------- | ------------ |
| function | The name of the aggregation function to use. | 'Sum' (required) |
| name | The name of the column to aggregate. | string (required) |

### QueryComparisonExpression

| Name | Description | Value |
| ---- | ----------- | ------------ |
| name | The name of the column to use in comparison. | string (required) |
| operator | The operator to use for comparison. | 'In' (required) |
| values | Array of values to use for comparison | string[] (required) |

### QueryDataset

| Name | Description | Value |
| ---- | ----------- | ------------ |
| aggregation | Dictionary of aggregation expression to use in the query. The key of each item in the dictionary is the alias for the aggregated column. Query can have up to 2 aggregation clauses. | [QueryDatasetAggregation](#querydatasetaggregation-2) |
| configuration | Has configuration information for the data in the export. The configuration will be ignored if aggregation and grouping are provided. | [QueryDatasetConfiguration](#querydatasetconfiguration-2) |
| filter | The filter expression to use in the query. Please reference our Query API REST documentation for how to properly format the filter. | [QueryFilter](#queryfilter-2) |
| granularity | The granularity of rows in the query. | 'Daily'<br />'Hourly' |
| grouping | Array of group by expression to use in the query. Query can have up to 2 group by clauses. | [QueryGrouping](#querygrouping-2)[] |
| sorting | Array of sorting by columns in query. | [QuerySortingConfiguration](#querysortingconfiguration-2)[] |

### QueryDatasetAggregation

| Name | Description | Value |
| ---- | ----------- | ------------ |

### QueryDatasetConfiguration

| Name | Description | Value |
| ---- | ----------- | ------------ |
| columns | Array of column names to be included in the query. Any valid query column name is allowed. If not provided, then query includes all columns. | string[] |

### QueryDefinition

| Name | Description | Value |
| ---- | ----------- | ------------ |
| dataset | Has definition for data in this query. | [QueryDataset](#querydataset-2) |
| timeframe | The time frame for pulling data for the query. If custom, then a specific time period must be provided. | 'BillingMonthToDate'<br />'Custom'<br />'MonthToDate'<br />'TheLastBillingMonth'<br />'TheLastMonth'<br />'TheLastWeek'<br />'TheLastYear'<br />'WeekToDate'<br />'YearToDate' (required) |
| timePeriod | Has time period for pulling data for the query. | [QueryTimePeriod](#querytimeperiod-2) |
| type | The type of the query. | 'Usage' (required) |

### QueryFilter

| Name | Description | Value |
| ---- | ----------- | ------------ |
| and | The logical "AND" expression. Must have at least 2 items. | [QueryFilter](#queryfilter-2)[] |
| dimension | Has comparison expression for a dimension | [QueryComparisonExpression](#querycomparisonexpression-2) |
| not | The logical "NOT" expression. | [QueryFilter](#queryfilter-2) |
| or | The logical "OR" expression. Must have at least 2 items. | [QueryFilter](#queryfilter-2)[] |
| tag | Has comparison expression for a tag | [QueryComparisonExpression](#querycomparisonexpression-2) |

### QueryGrouping

| Name | Description | Value |
| ---- | ----------- | ------------ |
| name | The name of the column to group. | string (required) |
| type | Has type of the column to group. | 'Dimension'<br />'Tag' (required) |

### QuerySortingConfiguration

| Name | Description | Value |
| ---- | ----------- | ------------ |
| name | The name of the column to use in sorting. | string |
| querySortingDirection | The sorting direction | 'Ascending'<br />'Descending' |

### QueryTimePeriod

| Name | Description | Value |
| ---- | ----------- | ------------ |
| from | The start date to pull data from. | string (required) |
| to | The end date to pull data to. | string (required) |


::: zone-end
