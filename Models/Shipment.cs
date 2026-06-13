using System;
using System.Collections.Generic;

namespace LogisticsSystem.Models
{
    public partial class Shipment
    {
        public int ShipmentId { get; set; }
        public string TrackingNumber { get; set; } = null!;
        public string SenderName { get; set; } = null!;
        public string? SenderPhone { get; set; }
        public string ReceiverName { get; set; } = null!;
        public string? ReceiverPhone { get; set; }
        public string? ReceiverAddress { get; set; }
        
        public decimal? Weight { get; set; }
        public string? Status { get; set; }
        public string? CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; }
        public string? Note { get; set; }
        
        public string? ProblemType { get; set; }
        public string? ProblemDescription { get; set; }

        // --- ВРЪЗКИ ---

        //Връзка с КУРИЕР
        public int? AssignedCourierId { get; set; }
        public virtual Courier? AssignedCourier { get; set; }

        //Връзка със СКЛАД
        public int? CurrentWarehouseId { get; set; } 
        public virtual Warehouse? CurrentWarehouse { get; set; }
        
      
    }
}