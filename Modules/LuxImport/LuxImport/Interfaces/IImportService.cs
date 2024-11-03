using Luxoria.Modules.Models.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxImport.Interfaces
{
    public interface IImportService
    {
        /// <summary>
        /// Verifies if the collection has already been initialized.
        /// </summary>
        /// <returns>True if the collection has been initialized, false otherwise.</returns>
        bool IsInitialized();

        ///<summary>
        /// Initializes the collection's database.
        ///</summary>
        void InitializeDatabase();
    }
}
