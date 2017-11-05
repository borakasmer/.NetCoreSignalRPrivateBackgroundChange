using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using coreSignalrPrivate.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Http;

namespace coreSignalrPrivate.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserName") == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        public ActionResult LogIn()
        {
            return View();
        }
        [HttpPost]
        public ActionResult LogIn(string name)
        {
            //Bu Nick daha önceden alındı mı?            
            if (!UserList.HasNick(name) && name.Trim() != "")
            {
                HttpContext.Session.SetString("UserName", name);
                return RedirectToAction("Index");
            }
            else
            {
                return Content("Error");
            }
        }
    }
    public class Background : Hub
    {
        public override Task OnConnectedAsync()
        {
            return Clients.Client(Context.ConnectionId).InvokeAsync("SetConnectionId", Context.ConnectionId);
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            var connectionID = Context.ConnectionId;
            UserList.Remove(connectionID);
            return Clients.All.InvokeAsync("SetListDisconnect", UserList._UserList);
        }
        public Task AddUserList(string userName, string connectionID)
        {
            UserList.Add(userName, connectionID);            
            return Clients.All.InvokeAsync("SetList", UserList._UserList);
        }
        public Task ChangeBackground(string connectionID,string image)
        {
            return Clients.Client(connectionID).InvokeAsync("ChangeImage", image);
        }
    }
    public static class UserList
    {
        static private Dictionary<string, string> _userlist = new Dictionary<string, string>();

        public static Dictionary<string, string> _UserList
        {
            get
            {
                return _userlist;
            }
        }
        public static void Add(string Nick, string ConnectionID)
        {       
            _UserList[Nick] = ConnectionID;       
        }
        public static void Remove(string ConnectionID)
        {
            //string key = _UserList.Where(cl => cl.Value == ConnectionID).Select(re => re.Key).FirstOrDefault();
            string key=GetNickByConnectionID(ConnectionID);
            if (key != null)
            {
                _UserList.Remove(key);
            }
        }
        public static bool HasNick(string Nick)
        {
            return _UserList.Any(cl => cl.Key.ToUpper() == Nick.ToUpper());
        }
        public static string GetNickByConnectionID(string ConnectionID)
        {
            return _UserList.FirstOrDefault(cl => cl.Value == ConnectionID).Key;
        }
    }
}
