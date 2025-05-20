import React from 'react';
import ExpandableCard from './ExpandableCard';
import type { PropertyDiff, FlagsChange } from '../../diffs/types';
import PropertyDiffList from './PropertyDiffList';
import ValueChangeView from './ValueChangeView';

interface PropertyDiffCardProps {
    name: string;
    diff: PropertyDiff;
    prefix?: string;
}

const PropertyDiffCard: React.FC<PropertyDiffCardProps> = ({
    name,
    diff,
    prefix = 'Property'
}) => {
    const renderField = <T extends string | number | null>(
        label: string,
        change: ValueChange<T> | FlagsChange<string> | undefined
    ) => {
        if (!change) return null;
        return <p>{label}: {change.From ?? ''} → {change.To ?? ''}</p>;
    };

    return (
        <ExpandableCard
            title={`${prefix}: ${name}`}
            status={diff.DiffType}
            contentStyle={{
                gap: 8,
            }}
        >
            {renderField('Type', diff.Type)}
            <ValueChangeView label='Type' change={diff.Type} />
            <ValueChangeView label='StructClass' change={diff.StructClass} />
            <ValueChangeView label='PropertyClass' change={diff.PropertyClass} />
            <ValueChangeView label='ArrayDim' change={diff.ArrayDim} />
            <PropertyDiffList properties={diff.InnerProperties} />
        </ExpandableCard>
    );
};

export default PropertyDiffCard;
