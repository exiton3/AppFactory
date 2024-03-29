﻿using System.Collections.Generic;

namespace AppFactory.Framework.Domain.Paging
{
    public class SimplePagedResult<T>
    {
        public IEnumerable<T> Results { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }
}