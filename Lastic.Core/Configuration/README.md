Steps:

1. Download Elasticsearch from	
https://www.elastic.co/guide/en/elasticsearch/reference/current/windows.html
https://artifacts.elastic.co/downloads/elasticsearch/elasticsearch-5.3.0.zip

2. Unzip Elasticsearch

3. Set -Xss320k (32-bit Windows) or -Xss1m (64-bit Windows) in config/jvm.options 

4. Install Java and set JAVA_HOME path
- Install 64 bit version from https://java.com/en/download/manual.jsp
- Open Control panel -> System -> Edit the system environment variables
 -> Advanced -> Environment Variables -> System variables -> New
	Variable name: JAVA_HOME
	Variable value: C:\Program Files\Java\jre1.8.0_111

NOTE: If update Java version this variable value need to be updated too!!!!!!!!!!!

- Run \bin\elasticsearch-service.bat manager
 -> Java -> Java Virtual Machine: - check only
	C:\Program Files\Java\jre1.8.0_111\bin\server\jvm.dll

5. Run Command Prompt: bin\elasticsearch-service.bat install

6. Start elasticsearch-service-x64 and set to run Automatic

7. Run in browser http://127.0.0.1:9200/

8. Setup Project install package - done in visual studio

9. Add in web.config/app.config

<configuration>
	<configSections><!--config section node need to be the first node in the configuration node-->
		<section name="CustomElasticSearchSection" type="CustomElasticSearch.Configuration.CustomElasticSearchSection, CustomElasticSearch" />
	</configSections>

	<CustomElasticSearchSection host="http://localhost" port="9200">
		<index key="MainIndex" name="FirstIndex" />
		<index key="SubIndex" name="SecondIndex" />
	</CustomElasticSearchSection>

................

</configuration>
