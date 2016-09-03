1. Setup a blank installation of sitecore somewhere with a hostname of sitecorerenderings.local
2. Copy everything from the installation bin folder to Lib\Sitecore
3. Open solution
4. Amend app_config/include/renderingstest/serializationpath.config to point to the Unicorn folder in this repo.
5. Build Solution (hopefully this is fine)
6. Publish solution to your working folder
7. (download and) open jMeter
8. Open the jmeter tests found in .\JMeter Tests
9. Open perfmon and any other monitoring tools (dottrace / ants / whatever)
9. Enable one of the tests and run

Happy performance enhancements