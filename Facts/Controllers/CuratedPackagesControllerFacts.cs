﻿using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Web.Mvc;
using Moq;
using Xunit;

namespace NuGetGallery
{
    public class CuratedPackagesControllerFacts
    {
        public class TheGetCreateCuratedPackageFormAction
        {
            [Fact]
            public void WillReturn404IfTheCuratedFeedDoesNotExist()
            {
                var controller = new TestableCuratedPackagesController();
                controller.StubCuratedFeedByNameQry.Setup(stub => stub.Execute(It.IsAny<string>(), It.IsAny<bool>())).Returns((CuratedFeed)null);

                var result = controller.CreateCuratedPackageForm("aFeedName");

                Assert.IsType<HttpNotFoundResult>(result);
            }

            [Fact]
            public void WillReturn403IfTheCurrentUsersIsNotAManagerOfTheCuratedFeed()
            {
                var controller = new TestableCuratedPackagesController();
                controller.StubIdentity.Setup(stub => stub.Name).Returns("notAManager");

                var result = controller.CreateCuratedPackageForm("aFeedName") as HttpStatusCodeResult;

                Assert.NotNull(result);
                Assert.Equal(403, result.StatusCode);
            }

            [Fact]
            public void WillPushTheCuratedFeedNameIntoTheViewBag()
            {
                var controller = new TestableCuratedPackagesController();
                controller.StubCuratedFeed.Name = "theCuratedFeedName";

                var result = controller.CreateCuratedPackageForm("aFeedName") as ViewResult;

                Assert.NotNull(result);
                Assert.Equal("theCuratedFeedName", result.ViewBag.CuratedFeedName);
            }

            public class TestableCuratedPackagesController : TestableCuratedPackagesControllerBase
            {
                public TestableCuratedPackagesController()
                {
                    StubCuratedFeedByNameQry
                        .Setup(stub => stub.Execute(It.IsAny<string>(), It.IsAny<bool>()))
                        .Returns(StubCuratedFeed);
                }
            }
        }

        public class ThePostCuratedPackagesAction
        {
            [Fact]
            public void WillReturn404IfTheCuratedFeedDoesNotExist()
            {
                var controller = new TestableCuratedPackagesController();
                controller.StubCuratedFeedByNameQry.Setup(stub => stub.Execute(It.IsAny<string>(), It.IsAny<bool>())).Returns((CuratedFeed)null);

                var result = controller.CuratedPackages("aFeedName", new CreatedCuratedPackageRequest());

                Assert.IsType<HttpNotFoundResult>(result);
            }

            [Fact]
            public void WillReturn403IfTheCurrentUsersIsNotAManagerOfTheCuratedFeed()
            {
                var controller = new TestableCuratedPackagesController();
                controller.StubIdentity.Setup(stub => stub.Name).Returns("notAManager");

                var result = controller.CuratedPackages("aFeedName", new CreatedCuratedPackageRequest()) as HttpStatusCodeResult;

                Assert.NotNull(result);
                Assert.Equal(403, result.StatusCode);
            }

            [Fact]
            public void WillPushTheCuratedFeedNameIntoTheViewBagAndShowTheCreateCuratedPackageFormWithErrorsWhenModelStateIsInvalid()
            {
                var controller = new TestableCuratedPackagesController();
                controller.StubCuratedFeed.Name = "theCuratedFeedName";
                controller.ModelState.AddModelError("", "anError");

                var result = controller.CuratedPackages("aFeedName", new CreatedCuratedPackageRequest()) as ViewResult;

                Assert.NotNull(result);
                Assert.Equal("theCuratedFeedName", result.ViewBag.CuratedFeedName);
                Assert.Equal("CreateCuratedPackageForm", result.ViewName);
            }

            [Fact]
            public void WillPushTheCuratedFeedNameIntoTheViewBagAndShowTheCreateCuratedPackageFormWithErrorsWhenThePackageIdDoesNotExist()
            {
                var controller = new TestableCuratedPackagesController();
                controller.StubCuratedFeed.Name = "theCuratedFeedName";
                controller.StubPackageRegistrationByIdQry.Setup(stub => stub.Execute(It.IsAny<string>(), It.IsAny<bool>())).Returns((PackageRegistration)null);

                var result = controller.CuratedPackages("aFeedName", new CreatedCuratedPackageRequest()) as ViewResult;

                Assert.NotNull(result);
                Assert.Equal("theCuratedFeedName", result.ViewBag.CuratedFeedName);
                Assert.Equal(Strings.PackageWithIdDoesNotExist, controller.ModelState["PackageId"].Errors[0].ErrorMessage);
                Assert.Equal("CreateCuratedPackageForm", result.ViewName);
            }

            [Fact]
            public void WillCreateTheCuratedPackage()
            {
                var controller = new TestableCuratedPackagesController();
                controller.StubCuratedFeed.Key = 42;
                controller.StubPackageRegistration.Key = 1066;

                controller.CuratedPackages(
                    "aFeedName", 
                    new CreatedCuratedPackageRequest
                    {
                        PackageId = "thePackageId", Notes = "theNotes"
                    });

                controller.StubCreatedCuratedPackageCmd.Verify(stub => stub.Execute(
                    42,
                    1066,
                    true,
                    false,
                    "theNotes"));
            }

            [Fact]
            public void WillRedirectToTheCuratedFeedRouteAfterCreatingTheCuratedPackage()
            {
                var controller = new TestableCuratedPackagesController();

                var result = controller.CuratedPackages("aFeedName", new CreatedCuratedPackageRequest()) as RedirectToRouteResult;

                Assert.NotNull(result);
                Assert.Equal(RouteName.CuratedFeed, result.RouteName);
            }
            
            public class TestableCuratedPackagesController : TestableCuratedPackagesControllerBase
            {
                public TestableCuratedPackagesController()
                {
                    StubCuratedFeedByNameQry
                        .Setup(stub => stub.Execute(It.IsAny<string>(), It.IsAny<bool>()))
                        .Returns(StubCuratedFeed);
                    StubPackageRegistrationByIdQry
                        .Setup(stub => stub.Execute(It.IsAny<string>(), It.IsAny<bool>()))
                        .Returns(StubPackageRegistration);
                }
            }
        }

        public class ThePatchCuratedPackageAction
        {
            [Fact]
            public void WillReturn404IfTheCuratedFeedDoesNotExist()
            {
                var controller = new TestableCuratedPackagesController();
                controller.StubCuratedFeedByNameQry.Setup(stub => stub.Execute(It.IsAny<string>(), It.IsAny<bool>())).Returns((CuratedFeed)null);

                var result = controller.CuratedPackage("aCuratedFeedName", "aCuratedPackageId", new ModifyCuratedPackageRequest());

                Assert.IsType<HttpNotFoundResult>(result);
            }

            [Fact]
            public void WillReturn404IfTheCuratedPackageDoesNotExist()
            {
                var controller = new TestableCuratedPackagesController();
                controller.StubCuratedFeed.Packages = new[] { new CuratedPackage { PackageRegistration = new PackageRegistration() } };

                var result = controller.CuratedPackage("aCuratedFeedName", "aCuratedPackageId", new ModifyCuratedPackageRequest());

                Assert.IsType<HttpNotFoundResult>(result);
            }

            [Fact]
            public void WillReturn403IfTheCuratedPackageDoesNotExist()
            {
                var controller = new TestableCuratedPackagesController();
                controller.StubCuratedFeed.Managers = new[] {new User {Username = "notAManager"}};

                var result = controller.CuratedPackage("aCuratedFeedName", "aCuratedPackageId", new ModifyCuratedPackageRequest()) as HttpStatusCodeResult;

                Assert.NotNull(result);
                Assert.Equal(403, result.StatusCode);
            }

            [Fact]
            public void WillReturn400IfTheModelStateIsInvalid()
            {
                var controller = new TestableCuratedPackagesController();
                controller.ModelState.AddModelError("", "anError");

                var result = controller.CuratedPackage("aCuratedFeedName", "aCuratedPackageId", new ModifyCuratedPackageRequest()) as HttpStatusCodeResult;

                Assert.NotNull(result);
                Assert.Equal(400, result.StatusCode);
            }

            [Fact]
            public void WillModifyTheCuratedPackageWhenRequestIsValid()
            {
                var controller = new TestableCuratedPackagesController();
                controller.StubCuratedFeed.Key = 42;
                controller.StubCuratedFeed.Packages = new[] { new CuratedPackage { Key = 1066, PackageRegistration = new PackageRegistration { Id = "theCuratedPackageId" } } };

                controller.CuratedPackage("theCuratedFeedName", "theCuratedPackageId", new ModifyCuratedPackageRequest{ Included = true});

                controller.StubModifyCuratedPackageCommand.Verify(stub => stub.Execute(
                    42,
                    1066,
                    true));
            }

            public class TestableCuratedPackagesController : TestableCuratedPackagesControllerBase
            {
                public TestableCuratedPackagesController()
                {
                    StubCuratedFeed.Managers = new[] {new User {Username = "aUsername"}};
                    StubCuratedFeed.Packages = new[] {new CuratedPackage {PackageRegistration = new PackageRegistration {Id = "aCuratedPackageId"}}};
                    StubCuratedFeedByNameQry
                        .Setup(stub => stub.Execute(It.IsAny<string>(), It.IsAny<bool>()))
                        .Returns(StubCuratedFeed);
                }
            }
        }

        public abstract class TestableCuratedPackagesControllerBase : CuratedPackagesController
        {
            protected TestableCuratedPackagesControllerBase()
            {
                StubCreatedCuratedPackageCmd = new Mock<ICreatedCuratedPackageCommand>();
                StubCuratedFeed = new CuratedFeed { Key = 0, Name = "aName", Managers = new HashSet<User>( new []{ new User { Username = "aUsername" } }) };
                StubCuratedFeedByNameQry = new Mock<ICuratedFeedByNameQuery>();
                StubIdentity = new Mock<IIdentity>();
                StubModifyCuratedPackageCommand = new Mock<IModifyCuratedPackageCommand>();
                StubPackageRegistration = new PackageRegistration { Key = 0, Id = "anId" };
                StubPackageRegistrationByIdQry = new Mock<IPackageRegistrationByIdQuery>();

                StubIdentity.Setup(stub => stub.IsAuthenticated).Returns(true);
                StubIdentity.Setup(stub => stub.Name).Returns("aUsername");
            }

            public Mock<ICreatedCuratedPackageCommand> StubCreatedCuratedPackageCmd { get; set; }
            public CuratedFeed StubCuratedFeed { get; set; }
            public Mock<ICuratedFeedByNameQuery> StubCuratedFeedByNameQry { get; private set; }
            public Mock<IIdentity> StubIdentity { get; private set; }
            public Mock<IModifyCuratedPackageCommand> StubModifyCuratedPackageCommand { get; private set; }
            public PackageRegistration StubPackageRegistration { get; private set; }
            public Mock<IPackageRegistrationByIdQuery> StubPackageRegistrationByIdQry { get; private set; }

            protected override IIdentity Identity
            {
                get { return StubIdentity.Object; }
            }

            protected override T GetService<T>()
            {
                if (typeof(T) == typeof(ICreatedCuratedPackageCommand))
                    return (T)StubCreatedCuratedPackageCmd.Object;
                
                if (typeof(T) == typeof(ICuratedFeedByNameQuery))
                    return (T)StubCuratedFeedByNameQry.Object;

                if (typeof(T) == typeof(IModifyCuratedPackageCommand))
                    return (T)StubModifyCuratedPackageCommand.Object;

                if (typeof(T) == typeof(IPackageRegistrationByIdQuery))
                    return (T)StubPackageRegistrationByIdQry.Object;

                throw new Exception("Tried to get an unexpected service.");
            }
        }
    }
}
