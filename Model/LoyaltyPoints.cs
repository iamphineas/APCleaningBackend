namespace APCleaningBackend.Model
{
    public class LoyaltyPoints
    {
        public int LoyaltyPointID { get; set; }
        public int CustomerID { get; set; }  // Foreign Key to ApplicationUser (Customer)
        public int PointsEarned { get; set; }  // Points earned from the booking
        public int PointsRedeemed { get; set; }  // Points redeemed for discounts
        public int PointsBalance => PointsEarned - PointsRedeemed;  // Remaining balance of points

        // Navigation property
        public virtual ApplicationUser Customer { get; set; }
    }

}
