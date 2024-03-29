﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LuvDating.Models;
using Microsoft.AspNet.Identity;


namespace LuvDating.Controllers
{
    public class ProfileController : Controller
    {
        // GET: Profile
        public ActionResult Index()
        {
            var db = new ApplicationDbContext();
            var userId = User.Identity.GetUserId();
            var userInfo = db.Users.FirstOrDefault(a => a.Id == userId);

            try
            {
                return View(new ProfileIndexViewModel
                {
                    AccountId = userId,
                    Name = userInfo.Name,
                    Birth = userInfo.BirthDate,
                    Gender = userInfo.Gender,
                    Bio = userInfo.Bio,
                    Image = userInfo.ImageName,
                });
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProfileEditViewModel model)
        {
            var db = new ApplicationDbContext();
            var userId = User.Identity.GetUserId();
            var userInfo = db.Users.FirstOrDefault(p => p.Id == userId);

            userInfo.Email = model.Email;
            userInfo.UserName = model.Email;
            userInfo.Name = model.Name;
            userInfo.BirthDate = model.Birth;
            userInfo.Gender = model.Gender;
            if(model.Bio != null)
            {
                userInfo.Bio = model.Bio;
            }
            
            if (ModelState.IsValid)
            {
                db.SaveChanges();

                return RedirectToAction("Index", "Profile");
            }
            return View(model);
        }
       

        public ActionResult UserProfile(string id)
        {
            var db = new ApplicationDbContext();
            var chosenProfile = db.Users.FirstOrDefault(p => p.Id == id);
            var currentUser = User.Identity.GetUserId();
            if (id == currentUser)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return View(new ProfileIndexViewModel
                {
                    AccountId = chosenProfile.Id,
                    Name = chosenProfile.Name,
                    Gender = chosenProfile.Gender,
                    Birth = chosenProfile.BirthDate,
                    Bio = chosenProfile.Bio,
                    Image = chosenProfile.ImageName

                });
            }
        }
        [Authorize]
        public ActionResult FriendRequest(string id)
        {
            var db = new ApplicationDbContext();
            var currentUser = User.Identity.GetUserId();
            var recieverProfile = db.Users.FirstOrDefault(p => p.Id == id);
            var senderProfile = db.Users.FirstOrDefault(p => p.Id == currentUser);


            bool exists = false;

            

            var validatorList2 = db.FriendModels.SelectMany(p => p.Sender).ToList();
            var validatorList = db.Users.SelectMany(p => p.FriendList).ToList();

            for (int i = 0; i < validatorList.Count(); i++)
            {

                //Kontrollerar om denna kombination av Id redan finns i db
                if (validatorList[i].FriendRequestReciever == id && validatorList2[i].Id == currentUser 
                    || (validatorList[i].FriendRequestReciever == currentUser && validatorList2[i].Id == id))
                {
                    exists = true;
                    break;
                }
            }

            if (exists == false)
            {
                senderProfile.FriendList.Add(new FriendModel
                {
                    FriendRequestReciever = id,
                    Name = recieverProfile.Name,
                    pendingRequest = 0
                });

                var reciever = new FriendModel();
                reciever.Sender.Add(new ApplicationUser { Id = currentUser });


                TempData["notice"] = "Friendrequest sent";
            }
            else {
                TempData["notice"] = "Relationship already established";
            }

            db.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult DisplayFriendRequests()
        {
            var db = new ApplicationDbContext();
            var currentUser = User.Identity.GetUserId();
            var senderProfile = db.Users.FirstOrDefault(p => p.Id == currentUser);
            var query = db.FriendModels.Where(p => p.FriendRequestReciever == currentUser && p.pendingRequest == 0).SelectMany(p => p.Sender).ToList();
            var list = new SenderListModel
            {
                RequestsFrom = query
            };

            return View(list);
        }

        public ActionResult AcceptRequest(string id)
        {
            var db = new ApplicationDbContext();
            var currentUser = User.Identity.GetUserId();
            var currentProfile = db.FriendModels.FirstOrDefault(p => p.FriendRequestReciever == currentUser);
            var senderProfile = db.Users.FirstOrDefault(p => p.Id == id);

            var validatorList = db.FriendModels.SelectMany(p => p.Sender).ToList();
            var validatorList2 = db.Users.SelectMany(p => p.FriendList).ToList();

            for (int i = 0; i < validatorList.Count(); i++)
            {
                if (validatorList[i].Id == id && validatorList2[i].FriendRequestReciever == currentUser && validatorList2[i].pendingRequest == 0)
                {
                    validatorList2[i].pendingRequest = 1;
                    break;
                }
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult DeclineRequest(string id)
        {
            var db = new ApplicationDbContext();
            var currentUser = User.Identity.GetUserId();
            var currentProfile = db.FriendModels.FirstOrDefault(p => p.FriendRequestReciever == currentUser);
            var senderProfile = db.Users.FirstOrDefault(p => p.Id == id);
            var sender = db.FriendModels.SelectMany(p => p.Sender).ToList();
            var reciever = db.Users.SelectMany(p => p.FriendList).ToList();


            for (int i = 0; i < sender.Count(); i++)
            {  
                if (sender[i].Id == id && reciever[i].FriendRequestReciever == currentUser && reciever[i].pendingRequest == 0)
                {
                    reciever[i].pendingRequest = 2;
                    reciever[i].FriendRequestReciever = "DECLINED";
                    break;

                }
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult DisplayFriends()
        {
            var db = new ApplicationDbContext();
            var currentUser = User.Identity.GetUserId();
            var senderProfile = db.Users.FirstOrDefault(p => p.Id == currentUser);
            var query = db.FriendModels.Where(p => p.FriendRequestReciever == currentUser && p.pendingRequest == 1).SelectMany(p => p.Sender).ToList();
            var validationList2 = db.FriendModels.SelectMany(p => p.Sender).ToList();
            var validationList = db.Users.SelectMany(p => p.FriendList).ToList();

            if (validationList2 != null)
            {

                for (int i = 0; i < validationList.Count(); i++)
                {
                    if (validationList[i].pendingRequest == 1 && validationList[i].FriendRequestReciever != currentUser && validationList2[i].Id == currentUser)
                    {
                        var _lista = validationList[i].FriendRequestReciever;
                        
                        var profile = db.Users.FirstOrDefault(p => p.Id == _lista);
                        query.Add(profile);
                    }
                }
            }

            var list = new SenderListModel
            {
                RequestsFrom = query
            };

            return View(list);
        }

        public ActionResult DeleteFriend(string id)
        {
            var db = new ApplicationDbContext();
            var currentUser = User.Identity.GetUserId();
            var currentProfile = db.FriendModels.FirstOrDefault(p => p.FriendRequestReciever == currentUser);
            var senderProfile = db.Users.FirstOrDefault(p => p.Id == id);
            var sender = db.FriendModels.SelectMany(p => p.Sender).ToList();
            var reciever = db.Users.SelectMany(p => p.FriendList).ToList();

            for (int i = 0; i < sender.Count(); i++)
            {
                if ((sender[i].Id == id && reciever[i].FriendRequestReciever == currentUser && reciever[i].pendingRequest == 1) ||
                    (sender[i].Id == currentUser && reciever[i].FriendRequestReciever == id))
                {
                    reciever[i].pendingRequest = 2;
                    reciever[i].FriendRequestReciever = "DELETED";
                    break;
                }
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }


    }


}