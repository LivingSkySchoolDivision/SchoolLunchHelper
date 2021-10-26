pipeline {
    agent any
    environment {
        REPO_API = "lunch/api"
        REPO_WEBMAN = "lunch/webmanager"
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
        stage('Docker build - WebManager') {
            steps {
                dir("src") {
                    sh "docker build -f Dockerfile-Manager -t ${REPO_WEBMAN}:latest -t ${REPO_WEBMAN}:${TAG} ."
                }
            }
        }
        stage('Docker push') {
            steps {
                sh "docker push ${PRIVATE_REPO_API}:${TAG}"
                sh "docker push ${PRIVATE_REPO_API}:latest"
                sh "docker push ${REPO_WEBMAN}:${TAG}"
                sh "docker push ${REPO_WEBMAN}:latest"
            }
        }
    }
    post {
        always {
            deleteDir()
        }
    }
}