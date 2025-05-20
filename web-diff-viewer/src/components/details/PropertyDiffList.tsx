import React from 'react';
import PropertyDiffCard from './PropertyDiffCard';
import type { PropertyDiff } from '../../diffs/types';

interface PropertyListProps {
    properties?: Record<string, PropertyDiff>;
}

const PropertyDiffList: React.FC<PropertyListProps> = ({ properties }) => {
    return (
        <div>
            {Object.entries(properties ?? {}).map(([propName, diff]) => (
                <PropertyDiffCard key={propName} name={propName} diff={diff} />
            ))}
        </div>
    );
};

export default PropertyDiffList;
