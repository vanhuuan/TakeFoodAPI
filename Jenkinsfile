void setBuildStatus(String message, String context, String state) {
  // add a Github access token as a global 'secret text' credential on Jenkins with the id 'github-commit-status-token'
    withCredentials([string(credentialsId: 'takefoodapi-github', variable: 'TOKEN')]) {
      // 'set -x' for debugging. Don't worry the access token won't be actually logged
      // Also, the sh command actually executed is not properly logged, it will be further escaped when written to the log
        sh """
            set -x
            curl \"https://api.github.com/repos/org/repo/statuses/$GIT_COMMIT" \
                -v -H \"Authorization: token $TOKEN\"
                -H \"Content-Type: application/json\" \
                -X POST \
                -d \"{\\\"description\\\": \\\"$message\\\", \\\"state\\\": \\\"$state\\\", \\\"context\\\": \\\"$context\\\", \\\"target_url\\\": \\\"$BUILD_URL\\\"}\"
        """
    } 
}

pipeline {
    agent any
    environment {
        NEXUSKEY = "bar"
        NEXUSURL = "http://20.92.240.52:8081/repository/nuget-hosted"
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
                    try{
                        sh
                        setBuildStatus("Compiling", "compile", "pending");
                        sh "dotnet build"
                        sh "dotnet pack -p:PackageVersion=0.${env:BUILD_NUMBER}.0"
                        setBuildStatus("Build complete", "compile", "success");
                    }catch(err) {
                        setBuildStatus("Failed", "pl-compile", "failure");
                        echo "Failed: ${err}"
                    }
                }
            }
        }
        stage('Publish to Nexus repository') {
            steps {
                sh "dotnet nuget push TakeFoodAPI/bin/Debug/TakeFoodAPI.0.${env:BUILD_NUMBER}.0.nupkg --api-key ${env:NEXUSKEY} --source ${env:NEXUSURL}"
            }
        }
    }
}