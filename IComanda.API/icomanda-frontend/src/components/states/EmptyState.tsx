import { Package, RefreshCw } from 'lucide-react'
import React from 'react'
import { Button } from '../ui/button'

interface EmptyStateProps {
  title: string
  description: string
  actionText?: string
  onAction?: () => void
  icon?: React.ReactNode
}

const EmptyState: React.FC<EmptyStateProps> = ({
  title,
  description,
  actionText,
  onAction,
  icon = <Package className="w-8 h-8 text-textMuted" />
}) => {
  return (
    <div className="text-center py-12">
      <div className="w-16 h-16 bg-bgMuted rounded-full flex items-center justify-center mx-auto mb-4">
        {icon}
      </div>
      <h3 className="text-lg font-medium text-white mb-2">{title}</h3>
      <p className="text-textMuted mb-6">{description}</p>
      {actionText && onAction && (
        <Button
          onClick={onAction}
          variant="outline"
          className="inline-flex items-center space-x-2"
        >
          <RefreshCw className="w-4 h-4" />
          <span>{actionText}</span>
        </Button>
      )}
    </div>
  )
}

export default EmptyState

