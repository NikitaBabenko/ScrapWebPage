using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Enums
{
    /// <summary>
    /// "Останавливать при дублировании", "Игнорировать при дублировании", "Обновлять при дублировании"
    /// </summary>
    public enum ConflictResolveType
    {
        Stop = 0,
        Ignore = 1,
        Update = 2
    }
}
