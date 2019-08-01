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
                sh 'dotnet test --logger "trx;LogFileName=encryptedConfigTestResults.trx" /p:CollectCoverage=true /p:coverletOutputFormat=cobertura /p:exclude="[xunit.*]*"'
                
            }
            post {
                always {
                    xunit thresholds: [failed(unstableThreshold: '0'), skipped(unstableThreshold: '20')], tools: [MSTest(deleteOutputFiles: true, failIfNotNew: true, pattern: '**/encryptedConfigTestResults.trx', skipNoTestFiles: false, stopProcessingIfError: true)]
                    recordIssues tools: [msBuild(), taskScanner(highTags:'FIXME,HACK', normalTags:'TODO', includePattern: '**/*.cs')]

                    sh 'mkdir coverage'
                    sh '$HOME/.dotnet/tools/reportgenerator -reports:EConfig/EConfig.Tests/coverage.cobertura.xml -targetdir:\\coverage -reporttypes:html "-classfilters:-*DTO*"'
                    publishHTML([allowMissing: false, alwaysLinkToLastBuild: false, keepAll: true, reportDir: '\\coverage', reportFiles: 'index.htm', reportName: 'CodeCoverage', reportTitles: ''])
                    cobertura coberturaReportFile: 'EConfig/EConfig.Tests/coverage.cobertura.xml'
                }
            }
        }
    }
    post {
        always {
            sh 'ls -R'
            archiveArtifacts artifacts: "**/encryptedConfigTestResults.trx,EConfig/EConfig.Tests/coverage.cobertura.xml,coverage//**"
            cleanWs()
        }
    }
}
