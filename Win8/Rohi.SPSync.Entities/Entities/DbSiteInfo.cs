using System;
using Rohi.Apps.Core;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using SQLite;
using System.Threading.Tasks;

namespace Rohi.EBinder.Entities
{
	public class DbSiteInfo
	{
		public DbSiteInfo() {
		}
		
		private DbSiteInfo (ServerEdition edition, string url, string loginName, string password)
		{
			this.Edition		= edition;
			this.Url			= url;
			this.UserName		= loginName;
			this.Password		= password;
			
			this.CreatedDate	= DateTime.Now;
			this.LastAccessDate	= DateTime.Now;
			this.ModifiedDate	= DateTime.Now;
		}
		
		#region Key site information

		[PrimaryKey]
		public string			SiteId					{ get; private set; }
		public string 			Url						{ get; private set; }
		public string			Title					{ get; private set; }
		public string			Description				{ get; private set; }
		public ServerEdition 	Edition					{ get; private set; }

		public string			SlideSourceLibrary		{ get; private set; }
		public string			PresentationLibrary		{ get; private set; }
		public string			QAList					{ get; private set; }

		public string			UserName				{ get; private set; }
		public string			Password				{ get; private set; }
		public bool				AutoLogin				{ get; private set; }
		
		public DateTime			CreatedDate				{ get; private set; }
		public DateTime			ModifiedDate			{ get; private set; }
		public DateTime			LastAccessDate			{ get; private set; }
		public DateTime?		LastMetadataSyncDate	{ get; private set; }
		public DateTime?		LastSyncDate			{ get; private set; }
		
		#endregion
		
		#region Derived Properties
		
		private string _serverName;
		public string ServerName  {
			get {
				if (_serverName == null) {
					Uri serverUri = new Uri(Url);
					_serverName = serverUri.Host;
				}
				return _serverName;
			}
		}
		
		public bool IsSyched {
			get {
				return LastSyncDate.HasValue;
			}
		}
		
		public bool IsMetadataSyched {
			get {
				return LastSyncDate.HasValue;
			}
		}
		
		#endregion
	
		#region Helper Methods
		
		public void MetadataSyncCompleted() {
			this.LastMetadataSyncDate = DateTime.Now;
		}
		
		public void SyncCompleted(string slideSourceLibrary, string presentationsLibrary, string qaList) {
			this.SlideSourceLibrary = slideSourceLibrary;
			this.PresentationLibrary = presentationsLibrary;
			this.QAList = qaList;
			this.LastSyncDate = DateTime.Now;
		}
		
		public void SetCredentials (ServerEdition edition, string url, string loginName, string password, bool autoLogin)
		{
			this.Url			= url;
			this.Edition		= edition;
			this.UserName		= loginName;
			this.Password		= password;
			this.AutoLogin		= autoLogin;
		}
		
		public void SetSiteInfo(string siteId, string title, string description) {
			this.SiteId			= siteId;
			this.Title			= title;
			this.Description	= description;
			this.LastAccessDate	= DateTime.Now;
			this.ModifiedDate	= DateTime.Now;
		}

		public void LoadFromSharePointNode(XElement node) {
            var idString = node.GetAttributeValue("Id").ToGuidString();
            if (string.IsNullOrEmpty(idString))
            {
                idString = node.GetAttributeValue("Title");
            }
			SetSiteInfo(
				idString,
				node.GetAttributeValue("Title"),
				node.GetAttributeValue("Description")
			);
		}
	
		#endregion
		
		#region Crud functions
		
		public void Insert(IBinderDb db) {
			db.Insert(this);
		}
		
		public void InsertOrUpdate(IBinderDb db) {
			db.InsertOrUpdate(this);
		}
		
		public void Update(IBinderDb db) {
			db.Update(this);
		}
		
		#endregion

		#region Get functions

        public async static Task<IList<DbSiteInfo>> GetAllSites(IBinderDb db)
        {
            return await db.Query<DbSiteInfo>("SELECT * FROM DbSiteInfo");
        }

        public async static Task<DbSiteInfo> GetBySiteId(IBinderDb db, Guid siteId)
        {
            var items = await db.Query<DbSiteInfo>("SELECT * FROM DbSiteInfo WHERE SiteId = ?", siteId);
            return items.FirstOrDefault();
        }

        public async static Task<DbSiteInfo> GetByUrl(IBinderDb db, string url)
        {
            var items = await db.Query<DbSiteInfo>("SELECT * FROM DbSiteInfo WHERE Url = ?", url);
            return items.FirstOrDefault();
        }

        #endregion

        #region Creation Functions

        public static DbSiteInfo CreateNew(ServerEdition edition, string url, string loginName, string password)
        {
            return new DbSiteInfo(edition, url, loginName, password);
        }

        #endregion

	}
}

