using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PharmacyAPI.Models;

namespace PharmacyAPI.DataAccess.Configurations
{
    public sealed class OrderItemBatchAllocationConfiguration
        : IEntityTypeConfiguration<OrderItemBatchAllocation>
    {
        public void Configure(EntityTypeBuilder<OrderItemBatchAllocation> builder)
        {
            builder.HasKey(allocation => allocation.Id);
            builder.Property(allocation => allocation.Quantity).IsRequired();

            builder.HasOne(allocation => allocation.OrderItem)
                .WithMany(item => item.BatchAllocations)
                .HasForeignKey(allocation => allocation.OrderItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(allocation => allocation.ProductBatch)
                .WithMany(batch => batch.OrderAllocations)
                .HasForeignKey(allocation => allocation.ProductBatchId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
