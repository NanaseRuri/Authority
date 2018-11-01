using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Xml;
using Authority.Infrastruture;
using Authority.Models;

namespace Authority.Controllers
{
    public class FormsAuthenticationController : Controller
    {
        static AccountHelper account=new AccountHelper();

        // GET: FormsAuthentication
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        //returnUrl 用于未在 Login 处进行验证而直接访问带有 [Authorize] 修饰的视图时，在验证后可返回原来访问的地址。
        public ActionResult Login(AccountHelper helper,string returnUrl)
        {
            if (ModelState.IsValid)
            {
                if (FormsAuthentication.Authenticate(helper.UserName, helper.Password))
                {
                    account.UserName = helper.UserName;
                    account.Password = helper.Password;
                    FormsAuthentication.SetAuthCookie(helper.UserName,false);
                    return Redirect(returnUrl ?? Url.Action("AfterAuthority",account));
                }
                else
                {
                    ModelState.AddModelError("", "Error account or password");
                    return View();
                }
            }
            return View();
        }

        [Authorize]
        public ActionResult AfterAuthority(AccountHelper helper)
        {
            return View(helper);
        }

        [Authorize]
        public ActionResult Edit()
        {
            return View(account);
        }

        [HttpPost]
        [Authorize]
        public ActionResult Edit(AccountHelper changedAccount)
        {
            XmlDocument doc=new XmlDocument();
            //获得配置文件的全路径
            string strFileName = AppDomain.CurrentDomain.BaseDirectory+"Web.Config";
            doc.Load(strFileName);
            XmlNodeList nodes = doc.GetElementsByTagName("user");
            for (int i = 0; i < nodes.Count; i++)
            {
                string _name = nodes[i].Attributes["name"]==null?
                    "":nodes[i].Attributes["name"].Value;
                if (_name==account.UserName)
                {
                    nodes[i].Attributes["name"].Value = changedAccount.UserName;
                    nodes[i].Attributes["password"].Value = changedAccount.Password;
                    FormsAuthentication.SignOut();
                    break;
                }
            }
            //将修改后的 Web.config 进行保存
            doc.Save(strFileName);

            return View("AfterAuthority",changedAccount);
        }


        [AdminAccount]
        public ActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [AdminAccount]
        public ActionResult Add(AccountHelper newAccount)
        {
            XmlDocument doc = new XmlDocument();
            //获得配置文件的全路径
            string strFileName = AppDomain.CurrentDomain.BaseDirectory + "Web.Config";
            doc.Load(strFileName);
            XmlNodeList nodes = doc.GetElementsByTagName("user");
            XmlNode credentials = nodes[0].ParentNode;
            XmlNode child = nodes[0].Clone();

            child.Attributes["name"].Value = newAccount.UserName;
            child.Attributes["password"].Value = newAccount.Password;
            credentials.AppendChild(child);

            //将修改后的 Web.config 进行保存
            doc.Save(strFileName);

            return View("Create", newAccount);
        }
    }
}