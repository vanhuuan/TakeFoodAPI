pipeline {
    agent any

    stages {
        stage('Git Checkout') {
            steps {
                git branch: 'develop', credentialsId: 'f255f29b-bccc-42e1-ad60-ef7c56881dda', url: 'git@github.com:vanhuuan89/TakeFoodAPI.git'
            }
        }
        stage('Build') {
            steps {
                sh "dotnet build"
            }
        }
    }
}