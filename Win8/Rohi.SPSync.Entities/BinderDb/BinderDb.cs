//
// Copyright 2010 Miguel de Icaza
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.IO;
using Rohi.Apps.Core;
using System.Collections.Generic;
using TinyIoC;
using Windows.Storage;
using System.Threading.Tasks;

namespace Rohi.EBinder.Entities
{
    public class BinderDb : IBinderDb
    {
        private SQLite.SQLiteAsyncConnection Connection { get; set; }

        private static string _connectionString;
        private static string ConnectionString
        {
            get
            {
                if (_connectionString == null)
                {
                    _connectionString =
                        Path.Combine(ApplicationData.Current.LocalFolder.Path, "sync.db");
                }
                return _connectionString;
            }
        }

        public BinderDb()
        {
            // Connection resolution
            Connection = new SQLite.SQLiteAsyncConnection(ConnectionString);
        }

        public void CreateDatabaseIfNotExists()
        {
            CreateDatabase();
        }

        private async void CreateDatabase()
        {
            Log.LogMessage("Database init");
            await Connection.CreateTableAsync<DbSiteInfo>();
            await Connection.CreateTableAsync<DbDocument>();
            await Connection.CreateTableAsync<DbLibrary>();
            Log.LogMessage("Database finish");
        }

        #region IBinderDb implementation
        public Task<int> Insert(object obj)
        {
            return Connection.InsertAsync(obj);
        }

        public Task<int> InsertOrUpdate(object obj)
        {
            return Connection.InsertOrUpdateAsync(obj);
        }

        public Task<int> Update(object obj)
        {
            return Connection.UpdateAsync(obj);
        }

        public Task<T> Get<T>(object pk)
            where T : new()
        {
            return Connection.GetAsync<T>(pk);
        }

        public Task<List<T>> Query<T>(string query, params object[] args)
            where T : new()
        {
            return Connection.QueryAsync<T>(query, args);
        }
        #endregion
    }
}

