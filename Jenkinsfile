pipeline {
    agent any
    environment {
        REPO_API = "lunch/api"
        PRIVATE_REPO_API = "${PRIVATE_DOCKER_REGISTRY}/${REPO_API}"
        TAG = "${BUILD_TIMESTAMP}"
    }
    stages {
        stage('Git clone') {
            steps {
                git branch: 'RefactorCleanup',
                    url: 'https://github.com/LivingSkySchoolDivision/SchoolLunchHelper.git'
            }
        }
        stage('Docker build - API') {
            steps {
                dir("src") {
                    sh "docker build -f Dockerfile-API -t ${PRIVATE_REPO_API}:latest -t ${PRIVATE_REPO_API}:${TAG} ."
                }
            }
        }
        stage('Docker push') {
            steps {
                sh "docker push ${PRIVATE_REPO_API}:${TAG}"
                sh "docker push ${PRIVATE_REPO_API}:latest"
            }
        }
    }
    post {
        always {
            deleteDir()
        }
    }
}