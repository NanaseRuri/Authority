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
        //在此创建一个静态的 AccountHelper 对象用来确保其生命期在整个web应用程序运行时始终存在，
        //如果创建一个非静态的 AccountHelper 对象会在登录其他网页时重新初始化而导致在一个页面中保存的数据在另一个页面消失。
        static AccountHelper account=new AccountHelper();

        //登录视图
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        //returnUrl 用于未在 Login 处进行验证而直接访问带有 [Authorize] 修饰的视图时，在验证后可返回原来访问的地址。
        public ActionResult Login(AccountHelper helper,string returnUrl)
        {            
            //满足[Required]修饰
            if (ModelState.IsValid)
            {
                if (FormsAuthentication.Authenticate(helper.UserName, helper.Password))
                {
                    //保存当前账号信息
                    account.UserName = helper.UserName;
                    account.Password = helper.Password;

                    //保存Cookie
                    FormsAuthentication.SetAuthCookie(helper.UserName,false);
                    return Redirect(returnUrl ?? Url.Action("AfterAuthority",account));
                }
                else
                {
                    //当验证失败时，向 ModelState 添加错误
                    ModelState.AddModelError("", "Error account or password");
                    return View();
                }
            }
            return View();
        }

        //用来表示登录成功
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

            //找到配置文件中的所有 user 节点，找到与当前账户匹配的节点并进行修改
            XmlNodeList nodes = doc.GetElementsByTagName("user");
            for (int i = 0; i < nodes.Count; i++)
            {                
                string _name = nodes[i].Attributes["name"]==null?
                    "":nodes[i].Attributes["name"].Value;
                if (_name==account.UserName)
                {
                    nodes[i].Attributes["name"].Value = changedAccount.UserName;
                    nodes[i].Attributes["password"].Value = changedAccount.Password;

                    //清除缓存并退出循环
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
            //获取 user 节点的父节点
            XmlNode credentials = nodes[0].ParentNode;
            //获得任意一个 user 节点的深拷贝
            XmlNode newChild = nodes[0].Clone();

            newChild.Attributes["name"].Value = newAccount.UserName;
            newChild.Attributes["password"].Value = newAccount.Password;
            //将子结点添加到父节点
            credentials.AppendChild(newChild);

            //将修改后的 Web.config 进行保存
            doc.Save(strFileName);

            return View("Create", newAccount);
        }
    }
}