using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Rohi.EBinder.Entities
{
	public interface IBinderDb
	{
        Task<int> Insert(object obj);
        Task<int> InsertOrUpdate(object obj);
        Task<int> Update(object obj);

        Task<T> Get<T>(object pk) where T : new();
        Task<List<T>> Query<T>(string query, params object[] args) where T : new();

        void CreateDatabaseIfNotExists();
    }
}

