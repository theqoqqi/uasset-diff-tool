import React from 'react';
import ExpandableCard from './ExpandableCard';
import type {FlagsChange, PropertyDiff, ValueChange} from '../../diffs/types';

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
        >
            {renderField('Type', diff.Type)}
            {renderField('StructClass', diff.StructClass)}
            {renderField('PropertyClass', diff.PropertyClass)}
            {renderField('ArrayDim', diff.ArrayDim)}
            {renderField('Flags', diff.PropertyFlags)}
            {Object.entries(diff.InnerProperties ?? {}).map(([innerName, innerData]) => (
                <PropertyDiffCard
                    key={innerName}
                    name={innerName}
                    diff={innerData}
                    prefix="Inner property"
                />
            ))}
        </ExpandableCard>
    );
};

export default PropertyDiffCard;
