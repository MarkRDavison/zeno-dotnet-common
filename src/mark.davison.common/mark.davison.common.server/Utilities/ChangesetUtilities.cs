namespace mark.davison.common.server.Utilities;

public static class ChangesetUtilities
{
    public static T? Apply<T>(T? entity, EntityChangeset changeset) where T : BaseEntity, new()
    {
        if (typeof(T).AssemblyQualifiedName != changeset.Type)
        {
            return entity;
        }

        if (entity != null && entity.Id != changeset.EntityId)
        {
            return entity;
        }

        if (entity == null)
        {
            if (changeset.EntityChangeType == EntityChangeType.Add)
            {
                entity = new();
                entity.Id = changeset.EntityId;
            }
            else if (changeset.EntityChangeType == EntityChangeType.Delete)
            {
                return null;
            }
            else
            {
                throw new InvalidOperationException("Cannot modify null entity");
            }
        }

        if (changeset.EntityChangeType == EntityChangeType.Delete)
        {
            return null;
        }

        foreach (var pcs in changeset.PropertyChangesets)
        {
            Apply<T>(entity, pcs);
        }

        return entity;
    }

    public static void Apply<T>(T entity, PropertyChangeset changeset) where T : BaseEntity, new()
    {
        var type = typeof(T);
        var property = type.GetProperty(changeset.Name);

        if (property != null)
        {
            property.SetValue(entity, changeset.Value);
        }
    }

    public static bool CanMergeChangesets(EntityChangeset lhs, EntityChangeset rhs)
    {
        if (lhs.EntityId != rhs.EntityId || lhs.Type != rhs.Type)
        {
            return false;
        }

        if (lhs.EntityChangeType == EntityChangeType.Add)
        {
            if (rhs.EntityChangeType == EntityChangeType.Add)
            {
                return false;
            }
        }
        else if (lhs.EntityChangeType == EntityChangeType.Delete)
        {
            if (rhs.EntityChangeType == EntityChangeType.Delete)
            {
                return false;
            }
            else if (rhs.EntityChangeType == EntityChangeType.Modify)
            {
                return false;
            }
        }
        else if (lhs.EntityChangeType == EntityChangeType.Modify)
        {
            if (rhs.EntityChangeType == EntityChangeType.Add)
            {
                return false;
            }
        }

        if (lhs.EntityChangeType == EntityChangeType.Barrier || rhs.EntityChangeType == EntityChangeType.Barrier)
        {
            return lhs.EntityChangeType == rhs.EntityChangeType;
        }

        return true;
    }

    public static EntityChangeset? MergeIntoChangeset(EntityChangeset destination, EntityChangeset source)
    {
        if (
            destination.EntityChangeType == EntityChangeType.Add &&
            source.EntityChangeType == EntityChangeType.Delete)
        {
            return null;
        }
        else if (
            (destination.EntityChangeType == EntityChangeType.Add || destination.EntityChangeType == EntityChangeType.Modify) &&
            source.EntityChangeType == EntityChangeType.Modify)
        {
            MergePropertyChanges(destination, source);
            return destination;
        }
        else if (
            destination.EntityChangeType == EntityChangeType.Delete &&
            source.EntityChangeType == EntityChangeType.Add)
        {
            return source;
        }
        else if (
            destination.EntityChangeType == EntityChangeType.Modify &&
            source.EntityChangeType == EntityChangeType.Delete)
        {
            return null;
        }
        else if (
            destination.EntityChangeType == EntityChangeType.Barrier &&
            source.EntityChangeType == EntityChangeType.Barrier)
        {
            return destination;
        }
        else
        {
            throw new InvalidOperationException("Cannot merge changesets");
        }
    }

    private static void MergePropertyChanges(EntityChangeset destination, EntityChangeset source)
    {
        destination.PropertyChangesets.RemoveAll(_ => source.PropertyChangesets.Any(__ => __.Name == _.Name));
        destination.PropertyChangesets.AddRange(source.PropertyChangesets);
    }

    public static List<EntityChangeset> ConsolidateChangeSets(List<EntityChangeset> changes)
    {
        var result = new List<EntityChangeset>();

        var entityGroups = changes
            .GroupBy(_ => new Tuple<Guid, string>(_.EntityId, _.Type))
            .ToList();

        foreach (var entityGroup in entityGroups)
        {
            var current = entityGroup.First();

            foreach (var cs in entityGroup.Skip(1))
            {
                if (current != null && CanMergeChangesets(current, cs))
                {
                    current = MergeIntoChangeset(current, cs);
                }
                else
                {
                    if (current == null)
                    {
                        current = cs;
                    }
                    result.Add(current);
                    current = cs;
                }
            }

            if (current != null)
            {
                if (!result.Any() || result.Last().Id != current.Id)
                {
                    result.Add(current);
                }
            }
        }

        return result;
    }

    public static bool ValidateChangesets(List<EntityChangeset> changes)
    {
        var entityGroups = changes
            .GroupBy(_ => new Tuple<Guid, string>(_.EntityId, _.Type))
            .ToList();

        var entityState = new Dictionary<Tuple<Guid, string>, bool?>();

        foreach (var eg in entityGroups)
        {
            entityState[eg.Key] = null;

            foreach (var cs in eg)
            {
                switch (cs.EntityChangeType)
                {
                    case EntityChangeType.Add:
                        {
                            if (entityState[eg.Key] == true)
                            {
                                return false;
                            }

                            entityState[eg.Key] = true;
                        }
                        break;
                    case EntityChangeType.Modify:
                        {
                            if (entityState[eg.Key] == false)
                            {
                                return false;
                            }

                            entityState[eg.Key] = true;
                        }
                        break;
                    case EntityChangeType.Delete:
                        {
                            if (entityState[eg.Key] == false)
                            {
                                return false;
                            }

                            entityState[eg.Key] = false;
                        }
                        break;
                    default:
                        return false;
                }
            }
        }

        return true;
    }

    public static EntityChangeset GenerateChangeset<TEntity>(Guid id) where TEntity : BaseEntity, new()
    {
        var entityType = typeof(TEntity);

        var cs = new EntityChangeset
        {
            EntityId = id,
            Type = entityType.AssemblyQualifiedName!,
            EntityChangeType = EntityChangeType.Delete
        };

        return cs;
    }

    public static EntityChangeset GenerateChangeset<TEntity>(TEntity entity) where TEntity : BaseEntity, new()
    {
        var cs = GenerateChangeset<TEntity>(new TEntity()
        {
            Id = entity.Id
        }, entity);

        cs.EntityChangeType = EntityChangeType.Add;

        return cs;
    }

    public static EntityChangeset GenerateChangeset<TEntity>(TEntity existing, TEntity updated) where TEntity : BaseEntity, new()
    {
        var entityType = typeof(TEntity);

        var cs = new EntityChangeset
        {
            EntityId = existing.Id,
            Type = entityType.AssemblyQualifiedName!,
            EntityChangeType = EntityChangeType.Modify
        };

        var properties = entityType
            .GetProperties()
            .ToList();

        foreach (var p in properties)
        {
            var existingValue = p.GetValue(existing);
            var updatedValue = p.GetValue(updated);

            if ((existingValue == null && updatedValue != null) ||
                (existingValue != null && updatedValue == null) ||
                (existingValue != null && !existingValue.Equals(updatedValue)) ||
                (updatedValue != null && !updatedValue.Equals(existingValue)))
            {
                cs.PropertyChangesets.Add(new() { Name = p.Name, Value = updatedValue });
            }
        }

        return cs;
    }
}
