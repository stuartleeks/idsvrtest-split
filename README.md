# IdSvrTest-Split

Project to test combining the [Identity Server tutorial](http://identityserver4.readthedocs.io/en/release/quickstarts/0_overview.html) into a single Project

## Running
Start the idsvrtest-split project and navigate to http://localhost:5000/quickstarts

Log in as `alice` or `bob` with password `password`. That will return you to the homepage where the claims will be displayed.

To log out, click the username at the top of the page and log out.

## Status

* Logging in works (local user, Google and Facebook)
* Logging out works
  * Need to look at the post logout redirect as that isn't working
* JS Client works
