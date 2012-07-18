﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using THOK.Common.Ef.MappingStrategy;

namespace THOK.Wms.DbModel.Mapping
{
    public class StorageMap : EntityMappingBase<Storage>
    {
        public StorageMap()
            : base("Wms")
        {
            // Primary Key
            this.HasKey(t => t.StorageCode);

            // Properties
            this.Property(t => t.StorageCode)
                .IsRequired()
                .HasMaxLength(50);

            this.Property(t => t.CellCode)
                .IsRequired()
                .HasMaxLength(20);

            this.Property(t => t.ProductCode)
                .IsRequired()
                .HasMaxLength(10);

            this.Property(t => t.Quantity)
                .IsRequired();

            this.Property(t => t.StorageTime)
                .IsRequired();

            this.Property(t => t.Rfid)
                .HasMaxLength(100);

            this.Property(t => t.InFrozenQuantity)
                .IsRequired();

            this.Property(t => t.OutFrozenQuantity)
                .IsRequired();

            this.Property(t => t.IsLock)
                .IsRequired()
                .IsFixedLength()
                .HasMaxLength(1);

            this.Property(t => t.LockTag)
                .HasMaxLength(50);

            this.Property(t => t.IsActive)
                .IsRequired()
                .IsFixedLength()
                .HasMaxLength(1);

            this.Property(t => t.UpdateTime)
                .IsRequired();

            // Table & Column Mappings
            this.Property(t => t.StorageCode).HasColumnName(ColumnMap.Value.To("StorageCode"));
            this.Property(t => t.CellCode).HasColumnName(ColumnMap.Value.To("CellCode"));
            this.Property(t => t.ProductCode).HasColumnName(ColumnMap.Value.To("ProductCode"));
            this.Property(t => t.Quantity).HasColumnName(ColumnMap.Value.To("Quantity"));
            this.Property(t => t.StorageTime).HasColumnName(ColumnMap.Value.To("StorageTime"));
            this.Property(t => t.Rfid).HasColumnName(ColumnMap.Value.To("Rfid"));
            this.Property(t => t.InFrozenQuantity).HasColumnName(ColumnMap.Value.To("InFrozenQuantity"));
            this.Property(t => t.OutFrozenQuantity).HasColumnName(ColumnMap.Value.To("OutFrozenQuantity"));
            this.Property(t => t.IsLock).HasColumnName(ColumnMap.Value.To("IsLock"));
            this.Property(t => t.LockTag).HasColumnName(ColumnMap.Value.To("LockTag"));
            this.Property(t => t.IsActive).HasColumnName(ColumnMap.Value.To("IsActive"));
            this.Property(t => t.UpdateTime).HasColumnName(ColumnMap.Value.To("UpdateTime"));

            // Relationships
            this.HasRequired(t => t.cell)
                .WithMany(t => t.Storage)
                .HasForeignKey(d => d.CellCode)
                .WillCascadeOnDelete(false);

            this.HasRequired(t => t.product)
                .WithMany(t => t.Storages)
                .HasForeignKey(d => d.ProductCode)
                .WillCascadeOnDelete(false);
        }
    }
}
