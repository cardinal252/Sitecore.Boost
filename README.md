# Sitecore.Boost
Performance Improvements for the Sitecore Platform

This repository contains a whole wealth of potential patches for the Sitecore CMS to improve performance. Most of these are running in production instances of Sitecore.

The hope is that for those of you on older versions you can get some of the insight into what I have been doing and help with your installs. But the goal of the project is 
to help improve the performance of the product overall by experimentation (and hopefully using that to feed into Sitecore's support / product teams) 

To begin

1. Setup a blank installation of sitecore somewhere with a hostname of sitecore.boost.local (default is c:\runtime\sitecore.boost.local)
2. Copy everything from the installation bin folder to Lib\Sitecore
3. Open solution
4. Build Solution (hopefully this is fine)
5. Publish Sitecore.Boost.Core to your working folder
6. Setup a custom serialization folder for unicorn pointing to the Unicorn folder in this repo
7. Sync unicorn
8. Publish
9. (download and) Open jMeter
10. Open the jmeter tests found in .\JMeter Tests
11. Open perfmon and any other monitoring tools (dottrace / ants / whatever)
12. Enable one of the tests and run to get a benchmark figure for your sitecore installation
13. Apply any / all patches
14. Try again :D

Happy performance enhancements

Nuget Feeds

For Sitecore 8.1 (and most versions below)

https://www.myget.org/F/sitecoreboost81/api/v2
https://www.myget.org/F/sitecoreboost81/api/v3/index.json

For Sitecore 8.2:
https://www.myget.org/F/sitecoreboost82/api/v2
https://www.myget.org/F/sitecoreboost82/api/v3/index.json