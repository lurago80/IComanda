import React from 'react'
import { Card } from '../ui/card'

const SkeletonCard: React.FC = () => {
  return (
    <Card className="border-0 bg-card/50 backdrop-blur-sm">
      <div className="p-4">
        <div className="flex items-center space-x-4">
          <div className="w-12 h-12 bg-bgMuted rounded-2xl animate-pulse"></div>
          <div className="flex-1">
            <div className="h-4 bg-bgMuted rounded w-3/4 mb-2 animate-pulse"></div>
            <div className="h-3 bg-bgMuted rounded w-1/2 animate-pulse"></div>
          </div>
        </div>
      </div>
    </Card>
  )
}

export const SkeletonProductCard: React.FC = () => {
  return (
    <Card className="border-0 bg-card/50 backdrop-blur-sm">
      <div className="p-4">
        <div className="flex items-start space-x-4">
          <div className="w-16 h-16 bg-bgMuted rounded-2xl animate-pulse"></div>
          <div className="flex-1">
            <div className="h-4 bg-bgMuted rounded w-3/4 mb-2 animate-pulse"></div>
            <div className="h-3 bg-bgMuted rounded w-1/2 mb-2 animate-pulse"></div>
            <div className="flex items-center justify-between">
              <div className="h-4 bg-bgMuted rounded w-1/4 animate-pulse"></div>
              <div className="w-10 h-10 bg-bgMuted rounded-xl animate-pulse"></div>
            </div>
          </div>
        </div>
      </div>
    </Card>
  )
}

export default SkeletonCard

