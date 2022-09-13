﻿namespace mark.davison.common.persistence.tests.Entities;

public class Blog : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid AuthorId { get; set; }
    public virtual Author? Author { get; set; }

}