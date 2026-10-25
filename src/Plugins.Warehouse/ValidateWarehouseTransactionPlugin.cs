using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TALXIS.DevKit.Build.Dataverse.Plugin;

namespace Plugins.Warehouse
{
    [PluginStep("Create", "lab_warehousetransaction", PluginStage.PreValidation)]
    public class ValidateWarehouseTransactionPlugin : PluginBase
    {
        public ValidateWarehouseTransactionPlugin() : base(typeof(ValidateWarehouseTransactionPlugin))
        {
        }

        public ValidateWarehouseTransactionPlugin(string unsecureConfiguration, string secureConfiguration)
            : base(typeof(ValidateWarehouseTransactionPlugin))
        {
        }

        protected override void ExecuteDataversePlugin(ILocalPluginContext localPluginContext)
        {
            var context = localPluginContext.PluginExecutionContext;
            if (!context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity target) ||
                target.LogicalName != "lab_warehousetransaction")
            {
                return;
            }

            if (!target.Contains("lab_quantity") || !target.Contains("lab_itemid") ||
                !target.Contains("lab_transactiontype") || !(target["lab_transactiontype"] is OptionSetValue transactionType) ||
                transactionType.Value != 100000001)
            {
                return;
            }

            var quantity = (int)target["lab_quantity"];
            var itemReference = (EntityReference)target["lab_itemid"];
            var service = localPluginContext.OrgSvcFactory.CreateOrganizationService(context.UserId);
            var item = service.Retrieve("lab_warehouseitem", itemReference.Id, new ColumnSet("lab_availablequantity"));
            var available = item != null && item.Contains("lab_availablequantity")
                ? (int)item["lab_availablequantity"]
                : 0;

            if (quantity > available)
            {
                throw new InvalidPluginExecutionException(
                    $"Not enough product in stock. Available: {available}, requested: {quantity}.");
            }
        }
    }
}
