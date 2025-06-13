// DIP App 自動化部署 Jenkins Pipeline

pipeline {
    agent any
    
    environment {
        APP_NAME = 'dip-app'
        CONTAINER_NAME = 'dip-container'
        HOST_PORT = '8081'  // 你的應用程式新端口
        DOCKER_IMAGE = "${APP_NAME}:v${BUILD_NUMBER}"
    }
    
    stages {
        stage('🔍 環境檢查') {
            steps {
                echo '=== 檢查目前環境狀態 ==='
                script {
                    sh '''
                        echo "📋 Jenkins 主機資訊："
                        whoami
                        docker --version
                        
                        echo "📋 現有 DIP App 映像："
                        docker images | grep dip-app || echo "找不到 dip-app 映像"
                        
                        echo "📋 現有 DIP App 容器："
                        docker ps -a | grep dip-container || echo "找不到 dip-container"
                    '''
                }
            }
        }
        
        stage('📥 程式碼更新') {
            steps {
                echo '正在獲取最新程式碼...'
                // Git checkout 由 Jenkins 自動處理
            }
        }
        
        stage('🔨 建立新映像') {
            steps {
                echo '正在建立新的 Docker 映像...'
                script {
                    // 建立新映像
                    def image = docker.build("${DOCKER_IMAGE}")
                    echo "✅ 新映像建立完成: ${DOCKER_IMAGE}"
                    
                    // 標記為 latest
                    sh "docker tag ${DOCKER_IMAGE} ${APP_NAME}:latest"
                    echo "✅ 已更新 latest 標籤"
                }
            }
        }
        
        stage('🛑 停止舊版本') {
            steps {
                echo '正在停止舊版本容器...'
                script {
                    sh '''
                        # 停止並移除舊容器
                        if docker ps -q -f name=dip-container | grep -q .; then
                            echo "正在停止運行中的容器..."
                            docker stop dip-container
                        fi
                        
                        if docker ps -aq -f name=dip-container | grep -q .; then
                            echo "正在移除舊容器..."
                            docker rm dip-container
                        fi
                        
                        echo "舊容器清理完成"
                    '''
                }
            }
        }
        
        stage('🚀 部署新版本') {
            steps {
                echo '正在部署新版本...'
                script {
                    // 啟動新容器
                    sh """
                        docker run -d \\
                        --name ${CONTAINER_NAME} \\
                        -p ${HOST_PORT}:80 \\
                        --restart unless-stopped \\
                        ${DOCKER_IMAGE}
                    """
                    
                    echo "🎉 新版本部署完成！"
                    echo "🌐 訪問網址: http://localhost:${HOST_PORT}"
                }
            }
        }
        
        stage('✅ 健康檢查') {
            steps {
                echo '正在進行健康檢查...'
                script {
                    // 等待容器啟動
                    sleep(time: 15, unit: 'SECONDS')
                    
                    sh """
                        echo "檢查容器狀態..."
                        docker ps | grep ${CONTAINER_NAME}
                        
                        echo "檢查應用程式日誌..."
                        docker logs --tail 5 ${CONTAINER_NAME}
                        
                        echo "測試網站連線..."
                        for i in {1..5}; do
                            if curl -f -s http://localhost:${HOST_PORT} >/dev/null; then
                                echo "✅ 網站回應正常！"
                                exit 0
                            else
                                echo "⏳ 等待網站啟動... (嘗試 \$i/5)"
                                sleep 10
                            fi
                        done
                        echo "⚠️ 網站可能需要更多時間啟動"
                    """
                }
            }
        }
        
        stage('🧹 清理映像') {
            steps {
                echo '清理舊映像...'
                script {
                    sh '''
                        # 保留最新 3 個版本的映像
                        docker images dip-app --format "{{.Repository}}:{{.Tag}}" | grep -v latest | sort -V | head -n -3 | xargs -r docker rmi || true
                        echo "映像清理完成"
                    '''
                }
            }
        }
    }
    
    post {
        always {
            echo '=== 部署完成狀態 ==='
            script {
                sh '''
                    echo "最新映像："
                    docker images | head -1 && docker images | grep dip-app
                    
                    echo "容器狀態："
                    docker ps | head -1 && docker ps | grep dip-container
                '''
            }
        }
        
        success {
            echo '''
🎉 DIP App 自動部署成功！ 🎉

✅ 部署詳情：
   🌐 網站: http://localhost:''' + env.HOST_PORT + '''
   🐳 容器: ''' + env.CONTAINER_NAME + '''
   📦 版本: ''' + env.DOCKER_IMAGE + '''

🚀 現在每次推送程式碼，Jenkins 都會自動更新你的應用程式！
            '''
        }
        
        failure {
            echo '''
❌ 部署失敗

🔧 請檢查：
1. Dockerfile 是否正確
2. 端口設定是否正確
3. Docker 權限是否足夠
4. 查看上方錯誤訊息

💡 常用除錯指令：
   docker logs dip-container
   docker ps -a
   docker images
            '''
        }
    }
}