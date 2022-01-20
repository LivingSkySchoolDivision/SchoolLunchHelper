pipeline {
    agent any
    environment {
        REPO_API = "lunch/api"
        REPO_WEBMAN = "lunch/webmanager"
        REPO_WEBUI = "lunch/webscanner"
        PRIVATE_REPO_API = "${PRIVATE_DOCKER_REGISTRY}/${REPO_API}"
        PRIVATE_REPO_WEBMAN = "${PRIVATE_DOCKER_REGISTRY}/${REPO_WEBMAN}"
        PRIVATE_REPO_WEBUI = "${PRIVATE_DOCKER_REGISTRY}/${REPO_WEBUI}"
        TAG = "${BUILD_TIMESTAMP}"
    }
    stages {
        stage('Git clone') {
            steps {
                git branch: 'main',
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
                    sh "docker build -f Dockerfile-Manager -t ${PRIVATE_REPO_WEBMAN}:latest -t ${PRIVATE_REPO_WEBMAN}:${TAG} ."
                }
            }
        }
        stage('Docker build - WebScanner') {
            steps {
                dir("src") {
                    sh "docker build -f Dockerfile-WebScanner -t ${PRIVATE_REPO_WEBUI}:latest -t ${PRIVATE_REPO_WEBUI}:${TAG} ."
                }
            }
        }
        stage('Docker push') {
            steps {
                sh "docker push ${PRIVATE_REPO_API}:${TAG}"
                sh "docker push ${PRIVATE_REPO_API}:latest"
                sh "docker push ${PRIVATE_REPO_WEBMAN}:${TAG}"
                sh "docker push ${PRIVATE_REPO_WEBMAN}:latest"
                sh "docker push ${PRIVATE_REPO_WEBUI}:${TAG}"
                sh "docker push ${PRIVATE_REPO_WEBUI}:latest"
            }
        }
    }
    post {
        always {
            deleteDir()
        }
    }
}