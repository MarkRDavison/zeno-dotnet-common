namespace mark.davison.common.server.tests.Utilities;

[TestClass]
public class ChangesetUtilitiesTests
{
    [TestMethod]
    public void Apply_WhereEntityIsNullAndChangesetTypeIsNotMatch_ReturnsNull()
    {
        TestEntity? existing = null;
        var updated = ChangesetUtilities.Apply<TestEntity>(
            existing,
            new EntityChangeset
            {
                EntityId = Guid.NewGuid(),
                Type = "NotTestEntity",
                EntityChangeType = EntityChangeType.Add
            });

        Assert.IsNull(updated);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Apply_WhereEntityIsNull_AndChangeTypeIsModify_Throws()
    {
        ChangesetUtilities.Apply<TestEntity>(
            null,
            new EntityChangeset
            {
                EntityId = Guid.NewGuid(),
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Modify
            });
    }

    [TestMethod]
    public void Apply_WhereEntityIsNotNull_AndChangeTypeIsDelete_ReturnsNull()
    {
        var id = Guid.NewGuid();
        var updated = ChangesetUtilities.Apply<TestEntity>(
            new TestEntity()
            {
                Id = id
            },
            new EntityChangeset
            {
                EntityId = id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Delete
            });

        Assert.IsNull(updated);
    }

    [TestMethod]
    public void Apply_WhereEntityIsNull_AndChangeTypeIsDelete_ReturnsNull()
    {
        var updated = ChangesetUtilities.Apply<TestEntity>(
            null,
            new EntityChangeset
            {
                EntityId = Guid.NewGuid(),
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Delete
            });

        Assert.IsNull(updated);
    }

    [TestMethod]
    public void Apply_WhereEntityIsNull_AndChangeTypeIAdd_CreatesEntity()
    {
        var changeset = new EntityChangeset
        {
            EntityId = Guid.NewGuid(),
            Type = typeof(TestEntity).AssemblyQualifiedName!,
            EntityChangeType = EntityChangeType.Add
        };
        var updated = ChangesetUtilities.Apply<TestEntity>(
            null,
            changeset);

        Assert.IsNotNull(updated);
        Assert.AreEqual(changeset.EntityId, updated.Id);
    }

    [TestMethod]
    public void Apply_WherePropertyChangesExist_ApplyToEntity()
    {
        var nameChange = new PropertyChangeset
        {
            Name = nameof(TestEntity.Name),
            Value = "New Name Value"
        };
        var descriptionChange = new PropertyChangeset
        {
            Name = nameof(TestEntity.Description),
            Value = "New Description Value"
        };

        var changeset = new EntityChangeset
        {
            EntityId = Guid.NewGuid(),
            Type = typeof(TestEntity).AssemblyQualifiedName!,
            EntityChangeType = EntityChangeType.Add,
            PropertyChangesets =
            {
                nameChange, descriptionChange
            }
        };
        var updated = ChangesetUtilities.Apply<TestEntity>(
            null,
            changeset);

        Assert.IsNotNull(updated);
        Assert.AreEqual(nameChange.Value, updated.Name);
        Assert.AreEqual(descriptionChange.Value, updated.Description);
    }

    [DataTestMethod]
    [DataRow("Entity1", "612A98CE-27BD-4B12-BE24-6A4730F40B37", "Entity2", "612A98CE-27BD-4B12-BE24-6A4730F40B37", false)]
    [DataRow("Entity1", "612A98CE-27BD-4B12-BE24-6A4730F40B37", "Entity1", "412A98CE-27BD-4B12-BE24-6A4730F40B32", false)]
    [DataRow("Entity1", "612A98CE-27BD-4B12-BE24-6A4730F40B37", "Entity1", "612A98CE-27BD-4B12-BE24-6A4730F40B37", true)]
    public void CanMergeChangesets_WithModifyChangeType_ReturnsCorrectly(string entity1Name, string entity1Id, string entity2Name, string entity2Id, bool valid)
    {
        Assert.AreEqual(valid, ChangesetUtilities.CanMergeChangesets(
            new()
            {
                EntityId = new Guid(entity1Id),
                Type = entity1Name,
                EntityChangeType = EntityChangeType.Modify
            },
            new()
            {
                EntityId = new Guid(entity2Id),
                Type = entity2Name,
                EntityChangeType = EntityChangeType.Modify
            }));
    }

    [DataTestMethod]
    [DataRow(EntityChangeType.Add, EntityChangeType.Add, false)]
    [DataRow(EntityChangeType.Add, EntityChangeType.Delete, true)]
    [DataRow(EntityChangeType.Add, EntityChangeType.Modify, true)]
    [DataRow(EntityChangeType.Add, EntityChangeType.Barrier, false)]
    [DataRow(EntityChangeType.Delete, EntityChangeType.Add, true)]
    [DataRow(EntityChangeType.Delete, EntityChangeType.Delete, false)]
    [DataRow(EntityChangeType.Delete, EntityChangeType.Modify, false)]
    [DataRow(EntityChangeType.Delete, EntityChangeType.Barrier, false)]
    [DataRow(EntityChangeType.Modify, EntityChangeType.Add, false)]
    [DataRow(EntityChangeType.Modify, EntityChangeType.Delete, true)]
    [DataRow(EntityChangeType.Modify, EntityChangeType.Modify, true)]
    [DataRow(EntityChangeType.Modify, EntityChangeType.Barrier, false)]
    [DataRow(EntityChangeType.Barrier, EntityChangeType.Add, false)]
    [DataRow(EntityChangeType.Barrier, EntityChangeType.Delete, false)]
    [DataRow(EntityChangeType.Barrier, EntityChangeType.Modify, false)]
    [DataRow(EntityChangeType.Barrier, EntityChangeType.Barrier, true)]
    public void CanMergeChangesets_WithDifferentChangeTypes_ReturnsCorrectly(EntityChangeType lhs, EntityChangeType rhs, bool valid)
    {
        Assert.AreEqual(
            valid,
            ChangesetUtilities.CanMergeChangesets(
                new()
                {
                    EntityId = Guid.Empty,
                    Type = "Entity",
                    EntityChangeType = lhs
                },
                new()
                {
                    EntityId = Guid.Empty,
                    Type = "Entity",
                    EntityChangeType = rhs
                }));
    }

    [TestMethod]
    public void MergeIntoChangeset_ForAddDelete_ReturnsNull()
    {
        var destination = new EntityChangeset
        {
            EntityId = Guid.Empty,
            Type = "Entity",
            EntityChangeType = EntityChangeType.Add
        };
        var source = new EntityChangeset
        {
            EntityId = Guid.Empty,
            Type = "Entity",
            EntityChangeType = EntityChangeType.Delete
        };

        var merged = ChangesetUtilities.MergeIntoChangeset(destination, source);

        Assert.IsNull(merged);
    }

    [TestMethod]
    public void MergeIntoChangeset_ForAddModify_ReturnsCorrectly_WhereNoDuplicateProperties()
    {
        var destination = new EntityChangeset
        {
            EntityId = Guid.Empty,
            Type = "Entity",
            EntityChangeType = EntityChangeType.Add,
            PropertyChangesets =
            {
                new() { Name = "Prop1", Value = "Val1" }
            }
        };
        var source = new EntityChangeset
        {
            EntityId = Guid.Empty,
            Type = "Entity",
            EntityChangeType = EntityChangeType.Modify,
            PropertyChangesets =
            {
                new() { Name = "Prop2", Value = "Val2" }
            }
        };

        var initialPropertyChanges = destination.PropertyChangesets.Count + source.PropertyChangesets.Count;

        var merged = ChangesetUtilities.MergeIntoChangeset(destination, source);

        Assert.IsNotNull(merged);
        Assert.AreEqual(destination.Id, merged.Id);
        Assert.AreEqual(initialPropertyChanges, merged.PropertyChangesets.Count);
    }

    [TestMethod]
    public void MergeIntoChangeset_ForAddModify_ReturnsCorrectly_WhereDuplicateProperties()
    {
        var destination = new EntityChangeset
        {
            EntityId = Guid.Empty,
            Type = "Entity",
            EntityChangeType = EntityChangeType.Add,
            PropertyChangesets =
            {
                new() { Name = "Prop1", Value = "Val1" }
            }
        };
        var source = new EntityChangeset
        {
            EntityId = Guid.Empty,
            Type = "Entity",
            EntityChangeType = EntityChangeType.Modify,
            PropertyChangesets =
            {
                new() { Name = "Prop1", Value = "Val2" }
            }
        };

        var merged = ChangesetUtilities.MergeIntoChangeset(destination, source);

        Assert.IsNotNull(merged);
        Assert.AreEqual(destination.Id, merged.Id);
        Assert.AreEqual(1, merged.PropertyChangesets.Count);
        Assert.AreEqual(source.PropertyChangesets[0].Value, merged.PropertyChangesets[0].Value);
    }

    [TestMethod]
    public void MergeIntoChangeset_ForDeleteAdd_ReturnsCorrectly()
    {
        var destination = new EntityChangeset
        {
            EntityId = Guid.Empty,
            Type = "Entity",
            EntityChangeType = EntityChangeType.Delete
        };
        var source = new EntityChangeset
        {
            EntityId = Guid.Empty,
            Type = "Entity",
            EntityChangeType = EntityChangeType.Add,
            PropertyChangesets =
            {
                new() { Name = "Prop2", Value = "Val2" }
            }
        };

        var merged = ChangesetUtilities.MergeIntoChangeset(destination, source);

        Assert.IsNotNull(merged);
        Assert.AreEqual(source.Id, merged.Id);
        Assert.AreEqual(source.PropertyChangesets.Count, merged.PropertyChangesets.Count);
    }

    [TestMethod]
    public void MergeIntoChangeset_ForModifyDelete_ReturnsCorrectly()
    {
        var destination = new EntityChangeset
        {
            EntityId = Guid.Empty,
            Type = "Entity",
            EntityChangeType = EntityChangeType.Modify,
            PropertyChangesets =
            {
                new() { Name = "Prop2", Value = "Val2" }
            }
        };
        var source = new EntityChangeset
        {
            EntityId = Guid.Empty,
            Type = "Entity",
            EntityChangeType = EntityChangeType.Delete
        };

        var merged = ChangesetUtilities.MergeIntoChangeset(destination, source);

        Assert.IsNull(merged);
    }

    [TestMethod]
    public void MergeIntoChangeset_ForModifyModify_ReturnsCorrectly_WhereNoDuplicateProperties()
    {
        var destination = new EntityChangeset
        {
            EntityId = Guid.Empty,
            Type = "Entity",
            EntityChangeType = EntityChangeType.Modify,
            PropertyChangesets =
            {
                new() { Name = "Prop1", Value = "Val1" }
            }
        };
        var source = new EntityChangeset
        {
            EntityId = Guid.Empty,
            Type = "Entity",
            EntityChangeType = EntityChangeType.Modify,
            PropertyChangesets =
            {
                new() { Name = "Prop2", Value = "Val2" }
            }
        };

        var initialPropertyChanges = destination.PropertyChangesets.Count + source.PropertyChangesets.Count;

        var merged = ChangesetUtilities.MergeIntoChangeset(destination, source);

        Assert.IsNotNull(merged);
        Assert.AreEqual(destination.Id, merged.Id);
        Assert.AreEqual(initialPropertyChanges, merged.PropertyChangesets.Count);
    }

    [TestMethod]
    public void MergeIntoChangeset_ForModifyModify_ReturnsCorrectly_WhereDuplicateProperties()
    {
        var destination = new EntityChangeset
        {
            EntityId = Guid.Empty,
            Type = "Entity",
            EntityChangeType = EntityChangeType.Modify,
            PropertyChangesets =
            {
                new() { Name = "Prop1", Value = "Val1" }
            }
        };
        var source = new EntityChangeset
        {
            EntityId = Guid.Empty,
            Type = "Entity",
            EntityChangeType = EntityChangeType.Modify,
            PropertyChangesets =
            {
                new() { Name = "Prop1", Value = "Val2" }
            }
        };

        var merged = ChangesetUtilities.MergeIntoChangeset(destination, source);

        Assert.IsNotNull(merged);
        Assert.AreEqual(destination.Id, merged.Id);
        Assert.AreEqual(1, merged.PropertyChangesets.Count);
        Assert.AreEqual(source.PropertyChangesets[0].Value, merged.PropertyChangesets[0].Value);
    }


    [TestMethod]
    public void MergeIntoChangeset_ForBarrierBarrier_ReturnsCorrectly()
    {
        var destination = new EntityChangeset
        {
            EntityId = Guid.Empty,
            Type = "Entity",
            EntityChangeType = EntityChangeType.Barrier
        };
        var source = new EntityChangeset
        {
            EntityId = Guid.Empty,
            Type = "Entity",
            EntityChangeType = EntityChangeType.Barrier
        };

        var merged = ChangesetUtilities.MergeIntoChangeset(destination, source);

        Assert.IsNotNull(merged);
        Assert.AreEqual(destination.Id, merged.Id);
    }


    [ExpectedException(typeof(InvalidOperationException))]
    [DataTestMethod]
    [DataRow(EntityChangeType.Add, EntityChangeType.Add)]
    [DataRow(EntityChangeType.Add, EntityChangeType.Barrier)]
    [DataRow(EntityChangeType.Delete, EntityChangeType.Delete)]
    [DataRow(EntityChangeType.Delete, EntityChangeType.Modify)]
    [DataRow(EntityChangeType.Delete, EntityChangeType.Barrier)]
    [DataRow(EntityChangeType.Modify, EntityChangeType.Add)]
    [DataRow(EntityChangeType.Modify, EntityChangeType.Barrier)]
    [DataRow(EntityChangeType.Barrier, EntityChangeType.Add)]
    [DataRow(EntityChangeType.Barrier, EntityChangeType.Delete)]
    [DataRow(EntityChangeType.Barrier, EntityChangeType.Modify)]
    public void MergeIntoChangeset_WithInvalidMergeType_Throws(EntityChangeType dst, EntityChangeType src)
    {
        var destination = new EntityChangeset
        {
            EntityId = Guid.Empty,
            Type = "Entity",
            EntityChangeType = dst
        };
        var source = new EntityChangeset
        {
            EntityId = Guid.Empty,
            Type = "Entity",
            EntityChangeType = src
        };

        ChangesetUtilities.MergeIntoChangeset(destination, source);
    }

    [TestMethod]
    public void ConsolidateChangeSets_WithSimpleInterleavedModifies_Works()
    {
        var entity1Id = Guid.NewGuid();
        var entity2Id = Guid.NewGuid();

        var changes = new List<EntityChangeset>
        {
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Modify,
                PropertyChangesets =
                {
                    new() { Name = nameof(TestEntity.Name), Value = "e1n1" }
                }
            },
            new()
            {
                EntityId = entity2Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Modify,
                PropertyChangesets =
                {
                    new() { Name = nameof(TestEntity.Name), Value = "e2n1" }
                }
            },
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Modify,
                PropertyChangesets =
                {
                    new() { Name = nameof(TestEntity.Name), Value = "e1n2" }
                }
            },
            new()
            {
                EntityId = entity2Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Modify,
                PropertyChangesets =
                {
                    new() { Name = nameof(TestEntity.Name), Value = "e2n2" }
                }
            },
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Modify,
                PropertyChangesets =
                {
                    new() { Name = nameof(TestEntity.Description), Value = "e1d1" }
                }
            },
            new()
            {
                EntityId = entity2Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Modify,
                PropertyChangesets =
                {
                    new() { Name = nameof(TestEntity.Description), Value = "e2d1" }
                }
            }
        };

        var consolidated = ChangesetUtilities.ConsolidateChangeSets(changes);

        Assert.AreEqual(2, consolidated.Count);
        Assert.AreEqual(entity1Id, consolidated[0].EntityId);
        Assert.AreEqual(2, consolidated[0].PropertyChangesets.Count);
        Assert.AreEqual(entity2Id, consolidated[1].EntityId);
        Assert.AreEqual(2, consolidated[1].PropertyChangesets.Count);
    }

    [TestMethod]
    public void ConsolidateChangeSets_WithAddDelete_Works()
    {
        var entity1Id = Guid.NewGuid();
        var entity2Id = Guid.NewGuid();

        var changes = new List<EntityChangeset>
        {
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Add,
                PropertyChangesets =
                {
                    new() { Name = nameof(TestEntity.Name), Value = "e1n1" }
                }
            },
            new()
            {
                EntityId = entity2Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Add,
                PropertyChangesets =
                {
                    new() { Name = nameof(TestEntity.Name), Value = "e2n1" }
                }
            },
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Delete
            },
            new()
            {
                EntityId = entity2Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Delete
            }
        };

        var consolidated = ChangesetUtilities.ConsolidateChangeSets(changes);

        Assert.AreEqual(0, consolidated.Count);
    }

    [TestMethod]
    public void ConsolidateChangeSets_WithDeleteAdd_Works()
    {
        var entity1Id = Guid.NewGuid();
        var entity2Id = Guid.NewGuid();

        var changes = new List<EntityChangeset>
        {
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Delete
            },
            new()
            {
                EntityId = entity2Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Delete
            },
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Add,
                PropertyChangesets =
                {
                    new() { Name = nameof(TestEntity.Name), Value = "e1n1" }
                }
            },
            new()
            {
                EntityId = entity2Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Add,
                PropertyChangesets =
                {
                    new() { Name = nameof(TestEntity.Name), Value = "e2n1" }
                }
            }
        };

        var consolidated = ChangesetUtilities.ConsolidateChangeSets(changes);

        Assert.AreEqual(2, consolidated.Count);
        Assert.AreEqual(entity1Id, consolidated[0].EntityId);
        Assert.AreEqual(1, consolidated[0].PropertyChangesets.Count);
        Assert.AreEqual(entity2Id, consolidated[1].EntityId);
        Assert.AreEqual(1, consolidated[1].PropertyChangesets.Count);
    }

    [TestMethod]
    public void ConsolidateChangeSets_WithAddDeleteAdd_Works()
    {
        var entity1Id = Guid.NewGuid();

        var changes = new List<EntityChangeset>
        {
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Add,
                PropertyChangesets =
                {
                    new() { Name = nameof(TestEntity.Name), Value = "e1n1" }
                }
            },
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Delete
            },
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Add,
                PropertyChangesets =
                {
                    new() { Name = nameof(TestEntity.Description), Value = "e1d1" }
                }
            }
        };

        var consolidated = ChangesetUtilities.ConsolidateChangeSets(changes);

        Assert.AreEqual(1, consolidated.Count);
        Assert.AreEqual(entity1Id, consolidated[0].EntityId);
        Assert.AreEqual(EntityChangeType.Add, consolidated[0].EntityChangeType);
        Assert.AreEqual(1, consolidated[0].PropertyChangesets.Count);
        Assert.AreEqual(nameof(TestEntity.Description), consolidated[0].PropertyChangesets[0].Name);
        Assert.AreEqual("e1d1", consolidated[0].PropertyChangesets[0].Value);
    }

    [TestMethod]
    public void ValidateChangesets_WhereOnlyModifies_ForSingleEntity_ReturnsTrue()
    {
        var entity1Id = Guid.NewGuid();

        var changes = new List<EntityChangeset>
        {
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Modify,
                PropertyChangesets =
                {
                    new() { Name = nameof(TestEntity.Name), Value = "e1n1" }
                }
            },
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Modify,
                PropertyChangesets =
                {
                    new() { Name = nameof(TestEntity.Name), Value = "e1n2" }
                }
            },
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Modify,
                PropertyChangesets =
                {
                    new() { Name = nameof(TestEntity.Description), Value = "e1d1" }
                }
            }
        };

        Assert.IsTrue(ChangesetUtilities.ValidateChangesets(changes));
    }

    [TestMethod]
    public void ValidateChangesets_WhereOnlyModifies_ForMultipleEntity_ReturnsTrue()
    {
        var entity1Id = Guid.NewGuid();
        var entity2Id = Guid.NewGuid();

        var changes = new List<EntityChangeset>
        {
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Modify,
                PropertyChangesets =
                {
                    new() { Name = nameof(TestEntity.Name), Value = "e1n1" }
                }
            },
            new()
            {
                EntityId = entity2Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Modify,
                PropertyChangesets =
                {
                    new() { Name = nameof(TestEntity.Name), Value = "e2n1" }
                }
            },
            new()
            {
                EntityId = entity2Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Modify,
                PropertyChangesets =
                {
                    new() { Name = nameof(TestEntity.Name), Value = "e2n2" }
                }
            },
            new()
            {
                EntityId = entity2Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Modify,
                PropertyChangesets =
                {
                    new() { Name = nameof(TestEntity.Description), Value = "e2d1" }
                }
            },
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Modify,
                PropertyChangesets =
                {
                    new() { Name = nameof(TestEntity.Name), Value = "e1n2" }
                }
            },
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Modify,
                PropertyChangesets =
                {
                    new() { Name = nameof(TestEntity.Description), Value = "e1d1" }
                }
            }
        };

        Assert.IsTrue(ChangesetUtilities.ValidateChangesets(changes));
    }

    [TestMethod]
    public void ValidateChangesets_WhereModifyAfterDelete_ReturnsFalse()
    {
        var entity1Id = Guid.NewGuid();

        var changes = new List<EntityChangeset>
        {
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Delete
            },
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Modify,
                PropertyChangesets =
                {
                    new() { Name = nameof(TestEntity.Name), Value = "e1n1" }
                }
            }
        };

        Assert.IsFalse(ChangesetUtilities.ValidateChangesets(changes));
    }

    [TestMethod]
    public void ValidateChangesets_WhereDeleteAfterDelete_ReturnsFalse()
    {
        var entity1Id = Guid.NewGuid();

        var changes = new List<EntityChangeset>
        {
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Delete
            },
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Delete
            }
        };

        Assert.IsFalse(ChangesetUtilities.ValidateChangesets(changes));
    }

    [TestMethod]
    public void ValidateChangesets_WhereAddAfterAdd_ReturnsFalse()
    {
        var entity1Id = Guid.NewGuid();

        var changes = new List<EntityChangeset>
        {
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Add
            },
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Add
            }
        };

        Assert.IsFalse(ChangesetUtilities.ValidateChangesets(changes));
    }

    [TestMethod]
    public void ValidateChangesets_WhereBarrierIncluded_ReturnsFalse()
    {
        var entity1Id = Guid.NewGuid();

        var changes = new List<EntityChangeset>
        {
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Add
            },
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Barrier
            }
        };

        Assert.IsFalse(ChangesetUtilities.ValidateChangesets(changes));
    }

    [TestMethod]
    public void ValidateChangesets_WhereMultipleAddModifyDelete_ReturnsTrue()
    {
        var entity1Id = Guid.NewGuid();

        var changes = new List<EntityChangeset>
        {
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Add
            },
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Modify
            },
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Delete
            },
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Add
            },
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Modify
            },
            new()
            {
                EntityId = entity1Id,
                Type = typeof(TestEntity).AssemblyQualifiedName!,
                EntityChangeType = EntityChangeType.Delete
            }
        };

        Assert.IsTrue(ChangesetUtilities.ValidateChangesets(changes));
    }

    [TestMethod]
    public void GenerateChangeset_WherePropertiesModifiedForModify_Works()
    {
        var existing = new TestEntity
        {
            Id = Guid.NewGuid(),
            Name = "Original value"
        };

        var updated = new TestEntity
        {
            Id = existing.Id,
            Name = "Updated value"
        };

        var cs = ChangesetUtilities.GenerateChangeset(existing, updated);

        Assert.AreEqual(EntityChangeType.Modify, cs.EntityChangeType);
        Assert.AreEqual(1, cs.PropertyChangesets.Count);
        Assert.AreEqual(nameof(TestEntity.Name), cs.PropertyChangesets[0].Name);
        Assert.AreEqual(updated.Name, cs.PropertyChangesets[0].Value);
    }

    [DataTestMethod]
    [DataRow(null, null, false)]
    [DataRow("val", null, true)]
    [DataRow(null, "val", true)]
    [DataRow("val", "val", false)]
    [DataRow("val1", "val2", true)]
    public void GenerateChangeset_WithNullablePropertyForModify_Works(string? lhs, string? rhs, bool change)
    {
        var existing = new TestEntity
        {
            Id = Guid.NewGuid(),
            Category = lhs
        };

        var updated = new TestEntity
        {
            Id = existing.Id,
            Category = rhs
        };

        var cs = ChangesetUtilities.GenerateChangeset(existing, updated);

        Assert.AreEqual(EntityChangeType.Modify, cs.EntityChangeType);

        var pcs = cs.PropertyChangesets.FirstOrDefault(_ => _.Name == nameof(TestEntity.Category));

        if (change)
        {
            Assert.IsNotNull(pcs);
            Assert.AreEqual(rhs, pcs.Value);
        }
        else
        {
            Assert.IsNull(pcs);
        }
    }

    [TestMethod]
    public void GenerateChangeset_WherePropertiesModifiedForAdd_Works()
    {
        var entity = new TestEntity
        {
            Id = Guid.NewGuid(),
            Category = null,
            Name = "name",
            Description = string.Empty
        };

        var cs = ChangesetUtilities.GenerateChangeset(entity);

        Assert.AreEqual(EntityChangeType.Add, cs.EntityChangeType);
        Assert.AreEqual(1, cs.PropertyChangesets.Count);
        Assert.AreEqual(nameof(TestEntity.Name), cs.PropertyChangesets[0].Name);
        Assert.AreEqual(entity.Name, cs.PropertyChangesets[0].Value);
    }

    [TestMethod]
    public void GenerateChangeset_ForDelete_Works()
    {
        var id = Guid.NewGuid();

        var cs = ChangesetUtilities.GenerateChangeset<TestEntity>(id);

        Assert.AreEqual(EntityChangeType.Delete, cs.EntityChangeType);
        Assert.AreEqual(0, cs.PropertyChangesets.Count);
    }
}
