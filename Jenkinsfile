void setBuildStatus(String message, String state) {
    step([
      $class: 'GitHubCommitStatusSetter',
      reposSource: [$class: 'ManuallyEnteredRepositorySource', url: 'https://github.com/vanhuuan89/TakeFoodAPI.git'],
      contextSource: [$class: 'ManuallyEnteredCommitContextSource', context: 'ci/jenkins/build-status'],
      errorHandlers: [[$class: 'ChangingBuildStatusErrorHandler', result: 'UNSTABLE']],
      statusResultSource: [ $class: 'ConditionalStatusResultSource', results: [[$class: 'AnyBuildResult', message: message, state: state]] ]
  ])
}

pipeline {
    agent any
    environment {
        NEXUSKEY = '7cddf6c9-6328-30ff-9927-3f903fbf86f0'
        NEXUSURL = 'http://20.205.40.63:8081/repository/nuget-hosted'
    }

    stages {
        stage('Git Checkout') {
            steps {
                cleanWs()
                setBuildStatus('Pending', 'PENDING')
                git branch: 'develop', credentialsId: 'takefoodapi-github', url: 'git@github.com:vanhuuan89/TakeFoodAPI.git'
            }
        }
        stage('Build and publish pack') {
            steps {
                script {
                    sh "echo ${env:BUILD_NUMBER}"
                    sh 'dotnet build'
                    sh "dotnet pack -p:PackageVersion=2.0.0"
                }
            }
        }
        stage('Publish to Nexus repository') {
            steps {
                sh "dotnet nuget push TakeFoodAPI/bin/Debug/TakeFoodAPI.2.0.0.nupkg --api-key ${env:NEXUSKEY} --source ${env:NEXUSURL}"
            }
        }
        stage('Devployment') {
            steps {
                sh 'sudo systemctl stop takefood'
                sh 'sudo rm -r /var/www/TakeFoodAPI/*'
                sh 'dotnet publish --configuration Release -o /var/www/TakeFoodAPI'
                sh 'sudo systemctl start takefood'
            }
        }
    }
    post {
        failure {
            setBuildStatus('Build Failed', 'FAILURE')
        }
        success {
            setBuildStatus('Build complete', 'SUCCESS')
        }
    }
}
