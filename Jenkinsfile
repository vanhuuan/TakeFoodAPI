// void setBuildStatus(String message, String context, String state) {
//     // add a Github access token as a global 'secret text' credential on Jenkins with the id 'github-commit-status-token'
//     withCredentials([string(credentialsId: 'takefoodapi-github', variable: 'TOKEN')]) {
//         // 'set -x' for debugging. Don't worry the access token won't be actually logged
//         // Also, the sh command actually executed is not properly logged, it will be further escaped when written to the log
//         sh """
//             set -x
//             curl \"https://api.github.com/repos/vanhuuan89/TakeFoodAPI/statuses/$GIT_COMMIT" \
//                 -v -H \"Authorization: Bearer $TOKEN\"
//                 -H \"Accept: application/vnd.github+json\" \
//                 -X POST \
//                 -d \"{\\\"description\\\": \\\"$message\\\", \\\"state\\\": \\\"$state\\\", \\\"context\\\": \\\"$context\\\", \\\"target_url\\\": \\\"$BUILD_URL\\\"}\"
//         """
//     }
// }

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
        NEXUSKEY = 'b3d2e853-339a-3b0d-b046-04dd841df65a'
        NEXUSURL = 'http://20.92.240.52:8081/repository/nuget-hosted'
    }

    stages {
        stage('Git Checkout') {
            steps {
                cleanWs()
                git branch: 'develop', credentialsId: 'f255f29b-bccc-42e1-ad60-ef7c56881dda', url: 'git@github.com:vanhuuan89/TakeFoodAPI.git'
            }
        }
        stage('Build and publish pack') {
            steps {
                script {
                    sh "echo ${env:BUILD_NUMBER}"
                    sh 'dotnet build'
                    sh "dotnet pack -p:PackageVersion=0.${env:BUILD_NUMBER}.0"
                }
            }
        }
        stage('Publish to Nexus repository') {
            steps {
                sh "dotnet nuget push TakeFoodAPI/bin/Debug/TakeFoodAPI.0.${env:BUILD_NUMBER}.0.nupkg --api-key ${env:NEXUSKEY} --source ${env:NEXUSURL}"
            }
        }
        stage('Devployment') {
            steps {
                sh 'sudo systemctl stop takefood'
                sh 'rm /var/www/TakeFoodAPI/*'
                sh 'dotnet publish --configuration Release -o /var/www/TakeFoodAPI'
                sh 'sudo systemctl start takefood'
            }
        }
    }
    post {
        failure {
            setBuildStatus('Build Failed', 'failure')
        }
        success {
            sh "echo ${env.BUILD_NUMBER}"
            setBuildStatus('Build complete', 'success')
        }
    }
}
