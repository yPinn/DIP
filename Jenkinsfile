pipeline {
    agent any
    
    environment {
        DOCKER_IMAGE = 'dip-app'
        CONTAINER_NAME = 'dip-container'
        HOST_PORT = '8081'
        CONTAINER_PORT = '80'
        BUILD_TIME = "${new Date().format('yyyy-MM-dd HH:mm:ss')}"
    }
    
    stages {
        stage('程式碼檢出') {
            steps {
                script {
                    echo "=== 📥 檢出 DIP 專案程式碼 ==="
                    
                    // 如果使用 Git，取消註解下一行
                    git branch: 'main', url: 'https://github.com/yPinn/DIP.git'
                    
                    echo "✅ 程式碼檢出完成"
                    sh '''
                        echo "專案結構:"
                        ls -la
                        
                        echo ""
                        echo "Controllers:"
                        ls -la Controllers/ 2>/dev/null || echo "Controllers 資料夾不存在"
                        
                        echo ""
                        echo "Views:"
                        find Views/ -name "*.cshtml" 2>/dev/null | head -5 || echo "Views 資料夾不存在"
                        
                        echo ""
                        echo "專案檔案:"
                        ls -la *.csproj 2>/dev/null || echo "找不到 .csproj 檔案"
                    '''
                }
            }
        }
        
        stage('程式碼分析') {
            steps {
                script {
                    echo "=== 🔍 DIP 專案程式碼分析 ==="
                    
                    sh '''
                        echo "檢查專案檔案..."
                        if [ -f "DIP.csproj" ]; then
                            echo "✅ 找到 DIP.csproj"
                            cat DIP.csproj | head -10
                        else
                            echo "❌ 找不到 DIP.csproj"
                            ls -la *.csproj
                        fi
                        
                        echo ""
                        echo "檢查 Controllers..."
                        CONTROLLERS=$(find Controllers/ -name "*.cs" 2>/dev/null | wc -l)
                        echo "發現 $CONTROLLERS 個 Controller 檔案"
                        
                        echo ""
                        echo "檢查 Models..."
                        MODELS=$(find Models/ -name "*.cs" 2>/dev/null | wc -l)
                        echo "發現 $MODELS 個 Model 檔案"
                        
                        echo ""
                        echo "檢查 Views..."
                        VIEWS=$(find Views/ -name "*.cshtml" 2>/dev/null | wc -l)
                        echo "發現 $VIEWS 個 View 檔案"
                    '''
                }
            }
        }
        
        stage('.NET 建置測試') {
            steps {
                script {
                    echo "=== 🔨 .NET 專案建置測試 ==="
                    
                    sh '''
                        # 檢查是否有 .NET SDK
                        if docker run --rm mcr.microsoft.com/dotnet/sdk:8.0 dotnet --version; then
                            echo "✅ .NET SDK 可用"
                        else
                            echo "❌ .NET SDK 不可用"
                            exit 1
                        fi
                        
                        # 在 Docker 容器中測試建置
                        echo "測試專案建置..."
                        docker run --rm -v $(pwd):/src -w /src mcr.microsoft.com/dotnet/sdk:8.0 sh -c "
                            echo '開始建置測試...'
                            dotnet restore
                            dotnet build -c Release --no-restore
                            echo '建置測試完成'
                        "
                    '''
                }
            }
        }
        
        stage('Docker 映像建立') {
            steps {
                script {
                    echo "=== 🐳 建立 DIP Docker 映像 ==="
                    
                    sh '''
                        echo "🏷️ 標記舊版本為備份..."
                        if docker images ${DOCKER_IMAGE}:latest -q | grep -q .; then
                            docker tag ${DOCKER_IMAGE}:latest ${DOCKER_IMAGE}:backup-$(date +%Y%m%d-%H%M%S)
                            echo "✅ 舊版本已備份"
                        else
                            echo "ℹ️ 沒有舊版本需要備份"
                        fi
                        
                        echo ""
                        echo "🔨 建立新的 Docker 映像..."
                        docker build \
                            --build-arg BUILD_TIME="${BUILD_TIME}" \
                            --build-arg BUILD_NUMBER="${BUILD_NUMBER}" \
                            -t ${DOCKER_IMAGE}:${BUILD_NUMBER} \
                            -t ${DOCKER_IMAGE}:latest \
                            .
                        
                        if [ $? -eq 0 ]; then
                            echo "✅ Docker 映像建立成功"
                        else
                            echo "❌ Docker 映像建立失敗"
                            exit 1
                        fi
                        
                        echo ""
                        echo "📋 映像資訊:"
                        docker images ${DOCKER_IMAGE}
                    '''
                }
            }
        }
        
        stage('映像測試') {
            steps {
                script {
                    echo "=== 🧪 Docker 映像測試 ==="
                    
                    sh '''
                        echo "🚀 啟動測試容器..."
                        TEST_CONTAINER="dip-test-${BUILD_NUMBER}"
                        TEST_PORT="8085"
                        
                        # 清理可能存在的測試容器
                        docker stop ${TEST_CONTAINER} 2>/dev/null || true
                        docker rm ${TEST_CONTAINER} 2>/dev/null || true
                        
                        # 啟動測試容器
                        docker run -d \
                            --name ${TEST_CONTAINER} \
                            -p ${TEST_PORT}:80 \
                            -e ASPNETCORE_ENVIRONMENT=Production \
                            ${DOCKER_IMAGE}:latest
                        
                        if [ $? -ne 0 ]; then
                            echo "❌ 測試容器啟動失敗"
                            exit 1
                        fi
                        
                        echo "⏳ 等待應用程式啟動..."
                        sleep 25
                        
                        echo "📋 測試容器日誌:"
                        docker logs ${TEST_CONTAINER} | tail -10
                        
                        echo ""
                        echo "🏥 執行健康檢查..."
                        HEALTH_OK=false
                        
                        for i in $(seq 1 8); do
                            echo "🔍 健康檢查 $i/8..."
                            
                            if curl -f -s -m 10 http://localhost:${TEST_PORT}/ > /dev/null; then
                                echo "✅ 應用程式回應正常！"
                                HEALTH_OK=true
                                break
                            fi
                            
                            echo "⏳ 等待 5 秒後重試..."
                            sleep 5
                        done
                        
                        # 清理測試容器
                        echo "🧹 清理測試容器..."
                        docker stop ${TEST_CONTAINER}
                        docker rm ${TEST_CONTAINER}
                        
                        # 檢查結果
                        if [ "$HEALTH_OK" = "true" ]; then
                            echo "✅ 映像測試通過，準備部署"
                        else
                            echo "❌ 映像測試失敗，停止部署"
                            exit 1
                        fi
                    '''
                }
            }
        }
        
        stage('生產部署') {
            steps {
                script {
                    echo "=== 🚀 部署到生產環境 ==="
                    
                    sh '''
                        echo "🛑 停止舊的生產容器..."
                        if docker ps -q -f name=${CONTAINER_NAME} | grep -q .; then
                            echo "停止運行中的容器: ${CONTAINER_NAME}"
                            docker stop ${CONTAINER_NAME}
                        else
                            echo "沒有運行中的容器需要停止"
                        fi
                        
                        if docker ps -a -q -f name=${CONTAINER_NAME} | grep -q .; then
                            echo "移除舊容器: ${CONTAINER_NAME}"
                            docker rm ${CONTAINER_NAME}
                        else
                            echo "沒有舊容器需要移除"
                        fi
                        
                        echo ""
                        echo "🗄️ 檢查 MySQL 容器..."
                        if ! docker ps -q -f name=mysql | grep -q .; then
                            echo "🚀 啟動 MySQL 容器..."
                            docker run -d \
                                --name mysql \
                                -e MYSQL_ROOT_PASSWORD=password \
                                -e MYSQL_DATABASE=DipDb \
                                -p 3306:3306 \
                                --restart unless-stopped \
                                mysql:8.0
                            
                            echo "⏳ 等待 MySQL 啟動..."
                            sleep 30
                        else
                            echo "✅ MySQL 容器已運行"
                        fi
                        
                        echo ""
                        echo "🚀 啟動新的生產容器..."
                        docker run -d \
                            --name ${CONTAINER_NAME} \
                            -p ${HOST_PORT}:${CONTAINER_PORT} \
                            --restart unless-stopped \
                            --link mysql:mysql \
                            -e ASPNETCORE_ENVIRONMENT=Production \
                            -e ASPNETCORE_URLS=http://+:${CONTAINER_PORT} \
                            -e BUILD_TIME="${BUILD_TIME}" \
                            -e BUILD_NUMBER="${BUILD_NUMBER}" \
                            -e CONNECTION_STRING="Server=mysql;Database=DipDb;User=root;Password=password;Port=3306;" \
                            ${DOCKER_IMAGE}:latest
                        
                        if [ $? -eq 0 ]; then
                            echo "✅ 生產容器啟動成功"
                        else
                            echo "❌ 生產容器啟動失敗"
                            exit 1
                        fi
                    '''
                }
            }
        }
        
        stage('部署驗證') {
            steps {
                script {
                    echo "=== ✅ 驗證生產部署 ==="
                    
                    sh '''
                        echo "⏳ 等待生產應用程式啟動..."
                        sleep 20
                        
                        echo "📊 生產容器狀態:"
                        docker ps -f name=${CONTAINER_NAME}
                        
                        echo ""
                        echo "📋 生產應用程式日誌:"
                        docker logs ${CONTAINER_NAME} | tail -15
                        
                        echo ""
                        echo "🏥 生產環境健康檢查:"
                        FINAL_CHECK=false
                        
                        for i in $(seq 1 6); do
                            echo "🔍 生產檢查 $i/6..."
                            
                            if curl -f -s -m 15 http://localhost:${HOST_PORT}/ > /dev/null; then
                                echo "✅ 生產環境健康檢查通過！"
                                FINAL_CHECK=true
                                break
                            fi
                            
                            echo "⏳ 等待 8 秒後重試..."
                            sleep 8
                        done
                        
                        if [ "$FINAL_CHECK" = "true" ]; then
                            echo "🎉 DIP 專案部署驗證成功！"
                        else
                            echo "❌ 生產環境健康檢查失敗"
                            echo "📋 詳細日誌:"
                            docker logs ${CONTAINER_NAME}
                            exit 1
                        fi
                    '''
                }
            }
        }
        
        stage('清理與優化') {
            steps {
                script {
                    echo "=== 🧹 清理舊資源 ==="
                    
                    sh '''
                        echo "🗑️ 清理舊 Docker 映像（保留最新 3 個版本）..."
                        
                        # 獲取所有 dip-app 映像，保留最新 3 個
                        OLD_IMAGES=$(docker images ${DOCKER_IMAGE} --format "{{.ID}}" | tail -n +4)
                        
                        if [ ! -z "$OLD_IMAGES" ]; then
                            echo "發現舊映像，準備清理:"
                            echo "$OLD_IMAGES"
                            echo "$OLD_IMAGES" | xargs docker rmi -f 2>/dev/null || echo "部分映像清理失敗（可能正在使用）"
                            echo "✅ 舊映像清理完成"
                        else
                            echo "ℹ️ 沒有需要清理的舊映像"
                        fi
                        
                        echo ""
                        echo "🧹 清理懸掛映像..."
                        docker image prune -f || echo "懸掛映像清理完成"
                        
                        echo ""
                        echo "📊 清理後的映像狀態:"
                        docker images ${DOCKER_IMAGE}
                    '''
                }
            }
        }
    }
    
    post {
        success {
            echo "🎉 =================================================="
            echo "✅ DIP ASP.NET Core MVC 專案部署成功！"
            echo "🎉 =================================================="
            echo "🌐 DIP 應用程式 URL: http://localhost:${HOST_PORT}"
            echo "📱 這是你真實的 ASP.NET Core MVC 專案"
            echo "📁 包含完整的 Controllers, Models, Views, Data"
            echo ""
            echo "🚀 成功完成的任務:"
            echo "   ✅ 程式碼檢出與分析"
            echo "   ✅ .NET 專案建置測試"
            echo "   ✅ Docker 映像建立"
            echo "   ✅ 映像功能測試"
            echo "   ✅ 生產環境部署"
            echo "   ✅ 部署狀態驗證"
            echo "   ✅ 資源清理優化"
            echo ""
            echo "📊 建置資訊:"
            echo "   🏷️ 版本: 1.0.${BUILD_NUMBER}"
            echo "   🕒 建置時間: ${BUILD_TIME}"
            echo "   🐳 容器: ${CONTAINER_NAME}"
            echo "   🌐 端口: ${HOST_PORT}"
            echo "🎉 =================================================="
        }
        
        failure {
            echo "❌ =================================================="
            echo "💥 DIP 專案部署失敗！"
            echo "❌ =================================================="
            
            sh '''
                echo "🔍 失敗診斷資訊:"
                echo "=================="
                
                echo "📋 容器狀態:"
                docker ps -a -f name=${CONTAINER_NAME}
                
                echo ""
                echo "📋 容器日誌:"
                docker logs ${CONTAINER_NAME} 2>/dev/null | tail -30 || echo "無法獲取容器日誌"
                
                echo ""
                echo "📋 映像狀態:"
                docker images ${DOCKER_IMAGE}
                
                echo ""
                echo "🔍 專案檔案:"
                ls -la *.csproj 2>/dev/null || echo "找不到專案檔案"
                
                echo "=================="
            '''
            
            echo "💡 可能的解決方案:"
            echo "   1. 檢查 .csproj 檔案是否正確"
            echo "   2. 確認 Controllers/Models/Views 結構"
            echo "   3. 檢查 Dockerfile 語法"
            echo "   4. 確認沒有編譯錯誤"
            echo "   5. 查看詳細的容器日誌"
            echo "❌ =================================================="
        }
        
        always {
            sh '''
                echo ""
                echo "📊 最終系統狀態摘要:"
                echo "==============================="
                echo "🐳 所有運行中的容器:"
                docker ps --format "table {{.Names}}\\t{{.Status}}\\t{{.Ports}}\\t{{.Image}}"
                
                echo ""
                echo "💿 DIP 相關映像:"
                docker images ${DOCKER_IMAGE} --format "table {{.Repository}}\\t{{.Tag}}\\t{{.Size}}\\t{{.CreatedSince}}"
                
                echo ""
                echo "💾 磁碟使用情況:"
                docker system df
                echo "==============================="
            '''
        }
    }
}