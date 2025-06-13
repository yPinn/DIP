pipeline {
    agent any
    
    environment {
        DOCKER_IMAGE = 'dip-app'
        CONTAINER_NAME = 'dip-container'
        HOST_PORT = '8081'
        CONTAINER_PORT = '8080'
    }
    
    stages {
        stage('檢查環境') {
            steps {
                script {
                    echo "=== 檢查目前運行環境 ==="
                    echo "Jenkins 工作目錄："
                    sh 'pwd'
                    sh 'ls -la'
                    echo "✅ 環境檢查完成"
                }
            }
        }
        
        stage('獲取程式碼') {
            steps {
                script {
                    echo "=== 獲取最新程式碼 ==="
                    echo "✅ 程式碼已由 Jenkins 自動獲取"
                    sh 'ls -la'
                }
            }
        }
        
        stage('建立部署腳本') {
            steps {
                script {
                    echo "=== 建立部署腳本 ==="
                    
                    // 建立部署腳本
                    writeFile file: 'deploy.ps1', text: '''
# 設定變數
$IMAGE_NAME = "dip-app"
$CONTAINER_NAME = "dip-container"
$HOST_PORT = "8081"
$CONTAINER_PORT = "8080"
$BUILD_NUMBER = $env:BUILD_NUMBER

Write-Host "=== DIP App 自動部署腳本 ===" -ForegroundColor Green
Write-Host "建置編號: $BUILD_NUMBER" -ForegroundColor Yellow

# 檢查 Docker 是否可用
Write-Host "🔍 檢查 Docker..." -ForegroundColor Blue
try {
    docker --version
    Write-Host "✅ Docker 可用" -ForegroundColor Green
} catch {
    Write-Host "❌ Docker 不可用" -ForegroundColor Red
    exit 1
}

# 停止並移除現有容器
Write-Host "🛑 停止現有容器..." -ForegroundColor Blue
$existingContainer = docker ps -q -f name=$CONTAINER_NAME
if ($existingContainer) {
    Write-Host "停止容器: $CONTAINER_NAME" -ForegroundColor Yellow
    docker stop $CONTAINER_NAME
    docker rm $CONTAINER_NAME
} else {
    Write-Host "沒有需要停止的容器" -ForegroundColor Gray
}

# 建立新映像
Write-Host "🔨 建立 Docker 映像..." -ForegroundColor Blue
docker build -t "${IMAGE_NAME}:latest" .
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ 映像建立失敗" -ForegroundColor Red
    exit 1
}
Write-Host "✅ 映像建立成功" -ForegroundColor Green

# 啟動新容器
Write-Host "🚀 啟動新容器..." -ForegroundColor Blue
docker run -d --name $CONTAINER_NAME -p "${HOST_PORT}:${CONTAINER_PORT}" --restart unless-stopped "${IMAGE_NAME}:latest"
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ 容器啟動失敗" -ForegroundColor Red
    exit 1
}

# 等待容器啟動
Write-Host "⏳ 等待容器啟動..." -ForegroundColor Blue
Start-Sleep -Seconds 10

# 檢查容器狀態
$runningContainer = docker ps -q -f name=$CONTAINER_NAME
if ($runningContainer) {
    Write-Host "✅ 容器成功啟動" -ForegroundColor Green
    Write-Host "🌐 應用程式可在 http://localhost:$HOST_PORT 訪問" -ForegroundColor Cyan
    
    # 顯示容器狀態
    Write-Host "容器狀態:" -ForegroundColor Blue
    docker ps -f name=$CONTAINER_NAME
} else {
    Write-Host "❌ 容器啟動失敗" -ForegroundColor Red
    Write-Host "容器日誌:" -ForegroundColor Yellow
    docker logs $CONTAINER_NAME
    exit 1
}

# 清理舊映像
Write-Host "🧹 清理舊映像..." -ForegroundColor Blue
$oldImages = docker images $IMAGE_NAME -q | Select-Object -Skip 1
if ($oldImages) {
    $oldImages | ForEach-Object { docker rmi $_ -f }
    Write-Host "✅ 舊映像清理完成" -ForegroundColor Green
} else {
    Write-Host "沒有需要清理的舊映像" -ForegroundColor Gray
}

Write-Host "🎉 部署完成！" -ForegroundColor Green
'''
                    
                    echo "✅ 部署腳本建立完成"
                }
            }
        }
        
        stage('執行部署') {
            steps {
                script {
                    echo "=== 執行部署 ==="
                    
                    // 在 Windows 宿主機上執行 PowerShell 腳本
                    bat '''
                        echo 執行部署腳本...
                        powershell -ExecutionPolicy Bypass -File deploy.ps1
                    '''
                    
                    echo "✅ 部署執行完成"
                }
            }
        }
        
        stage('驗證部署') {
            steps {
                script {
                    echo "=== 驗證部署結果 ==="
                    
                    // 檢查容器狀態
                    bat '''
                        echo 檢查容器狀態...
                        docker ps -f name=dip-container
                        
                        echo 檢查映像...
                        docker images | findstr dip-app
                    '''
                    
                    echo "✅ 部署驗證完成"
                }
            }
        }
    }
    
    post {
        always {
            script {
                echo "=== 建置後清理 ==="
                // 清理臨時檔案
                sh 'rm -f deploy.ps1 || true'
                
                echo "📊 最終狀態："
                bat '''
                    echo Docker 容器:
                    docker ps -f name=dip
                    
                    echo Docker 映像:
                    docker images | findstr dip
                '''
            }
        }
        
        success {
            echo "🎉 建置成功！"
            echo "🌐 請訪問 http://localhost:8081 查看應用程式"
        }
        
        failure {
            echo "❌ 建置失敗！請檢查日誌"
            bat '''
                echo 容器日誌:
                docker logs dip-container || echo 無法獲取容器日誌
            '''
        }
    }
}