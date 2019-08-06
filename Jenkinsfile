pipeline {
    options {
        disableConcurrentBuilds()
        timeout(15)
    }

    agent {
        label 'ubuntu'
    }
    environment {
        HOME = '/tmp'
        PROGET_CREDS = credentials('proget-nuget-push')
    }
    stages {
        stage('Install tools') { 
            steps {
                sh 'dotnet tool install -g dotnet-reportgenerator-globaltool'
            }
        }
        stage('Build') {
            steps {
                sh 'dotnet build -v normal'
            }
        }
        stage('Testing') {
            steps {
                sh 'dotnet test --logger "trx;LogFileName=econfigTestResults.trx" /p:CollectCoverage=true /p:coverletOutputFormat=cobertura /p:exclude="[xunit.*]*"'
                
            }
            post {
                always {
                    xunit thresholds: [failed(unstableThreshold: '0'), skipped(unstableThreshold: '20')], tools: [MSTest(deleteOutputFiles: true, failIfNotNew: true, pattern: '**/econfigTestResults.trx', skipNoTestFiles: false, stopProcessingIfError: true)]
                    recordIssues tools: [msBuild(), taskScanner(highTags:'FIXME,HACK', normalTags:'TODO', includePattern: '**/*.cs')]

                    sh 'mkdir coverage'
                    sh '$HOME/.dotnet/tools/reportgenerator -reports:EConfig.Tests/coverage.cobertura.xml -targetdir:\\coverage -reporttypes:html "-classfilters:-*DTO*"'
                    publishHTML([allowMissing: false, alwaysLinkToLastBuild: false, keepAll: true, reportDir: '\\coverage', reportFiles: 'index.htm', reportName: 'CodeCoverage', reportTitles: ''])
                    cobertura coberturaReportFile: 'EConfig.Tests/coverage.cobertura.xml'
                }
            }
        }
        stage('Publish') 
        {
            when {
                branch "master"
            }
            steps { 
                script {
                    def safe_build_number = getSafeBuildNumber(env.BUILD_NUMBER)
                    sh "dotnet pack -c Release /p:PackageVersion=1.${safe_build_number}"
                    sh "dotnet nuget push ./EConfig/nupkg/*.nupkg -k $PROGET_CREDS -s http://10.124.100.97:8080/nuget/NugetComponents"
                }
            }
            post {
                success {
                    scipt {
                        def commitUrl = "${env.GIT_URL.take(env.GIT_URL.length() - 4)}/commit/${env.GIT_COMMIT}"
                        def commit = env.GIT_COMMIT.take(7)
                        def logUrl = "${env.JOB_URL}/${env.BUILD_NUMBER}/console"
                        slackSend iconEmoji: '', message: ":shipit: ${env.GIT_AUTHOR_NAME} has deployed a new version of :unlock: econfig <$commitUrl|$commit> <$logUrl|Logs>", username: ''
                    }
                }
                failure {
                    script {
                        def commitUrl = "${env.GIT_URL.take(env.GIT_URL.length() - 4)}/commit/${env.GIT_COMMIT}"
                        def commit = env.GIT_COMMIT.take(7)
                        def logUrl = "${env.JOB_URL}/${env.BUILD_NUMBER}/console"
                        slackSend iconEmoji: '', message: ":x: ${env.GIT_AUTHOR_NAME} failed to deploy :unlock: econfig <$commitUrl|$commit> <$logUrl|Logs>", username: ''
                    }
                }
            }
        }
    }
    post {
        always {
            sh 'ls -R'
            archiveArtifacts artifacts: "**/encryptedConfigTestResults.trx,EConfig.Tests/coverage.cobertura.xml,coverage//**"
            cleanWs()
        }
    }
}

//This safe build number makes sure that a higher buildNo will always be considered a newer version by visual studio.  (up to 10000 builds)
String getSafeBuildNumber(String buildNo)
{
    while (buildNo.size() < 5) {
        String a = "0";
        buildNo = a + buildNo;
    }
    return buildNo;
}