using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace NorthwindCustomers.Models
{
    public class OrderInfo
    {
        [Display(Name ="Order ID")]
        public int OrderId { get; set; }
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; }
        [Display(Name = "Assigned To")]
        public string AssignedTo { get; set; }
        [Display(Name = "Order made on")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime? OrderDate { get; set; }
        [Display(Name = "Address")]
        public string ShipAddress { get; set; }
        [Display(Name = "City")]
        public string ShipCity { get; set; }
        [Display(Name = "Country")]

        public string ShipCountry { get; set; }
        [Display(Name = "Total cost")]
        public decimal TotalCost { get; set; }
    }
}
